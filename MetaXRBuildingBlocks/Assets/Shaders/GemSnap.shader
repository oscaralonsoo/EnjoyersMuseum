Shader "Custom/URP/GemSnapCrystal"
{
    Properties
    {
        [Header(Colors)]
        [HDR] _BaseColor    ("Base Color", Color)      = (0.2, 0.7, 1.0, 1.0)
        [HDR] _RimColor     ("Rim Color", Color)       = (0.6, 0.95, 1.0, 1.0)
        [HDR] _ActiveColor  ("Active / Valid Color", Color) = (0.3, 1.0, 0.5, 1.0)
        _Active             ("Active (0-1)", Range(0,1)) = 0

        [Header(Crystal)]
        _BaseAlpha          ("Base Alpha", Range(0,1))  = 0.12
        _FresnelPower       ("Fresnel Power", Range(0.5,8)) = 2.5
        _RimIntensity       ("Rim Intensity", Range(0,4))   = 1.5
        _FacetStrength      ("Facet Strength (flat shading)", Range(0,1)) = 1.0
        _FacetContrast      ("Facet Contrast", Range(0,1))  = 0.6
        _BackFaceIntensity  ("Back Face Intensity", Range(0,1)) = 0.5

        [Header(Sparkle)]
        _SparkleSpeed       ("Sparkle Speed", Range(0,10))  = 2.5
        _SparkleIntensity   ("Sparkle Intensity", Range(0,4)) = 1.5

        [Header(Pulse and Scanline)]
        _PulseSpeed         ("Pulse Speed", Range(0,10))    = 2.0
        _PulseAmount        ("Pulse Amount", Range(0,1))    = 0.25
        _ScanDensity        ("Scanline Density", Range(0,20)) = 6
        _ScanSpeed          ("Scanline Speed", Range(-5,5)) = 0.6
        _ScanIntensity      ("Scanline Intensity", Range(0,2)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4  _BaseColor;
            half4  _RimColor;
            half4  _ActiveColor;
            half   _Active;
            half   _BaseAlpha;
            half   _FresnelPower;
            half   _RimIntensity;
            half   _FacetStrength;
            half   _FacetContrast;
            half   _BackFaceIntensity;
            half   _SparkleSpeed;
            half   _SparkleIntensity;
            half   _PulseSpeed;
            half   _PulseAmount;
            half   _ScanDensity;
            half   _ScanSpeed;
            half   _ScanIntensity;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS   : TEXCOORD1;
            float  posOSy     : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings vert(Attributes IN)
        {
            Varyings OUT;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
            OUT.positionCS = vp.positionCS;
            OUT.positionWS = vp.positionWS;
            OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
            OUT.posOSy     = IN.positionOS.y;
            return OUT;
        }

        float Hash31(float3 p)
        {
            return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
        }

        half4 ShadeCrystal(Varyings IN, half intensityMul)
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

            float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);

            // Normal suave + normal plana (facetas de cristal) por derivadas
            float3 smoothN = normalize(IN.normalWS);
            float3 flatN   = normalize(cross(ddy(IN.positionWS), ddx(IN.positionWS)));
            if (dot(flatN, smoothN) < 0) flatN = -flatN;
            float3 N = normalize(lerp(smoothN, flatN, _FacetStrength));

            // Fresnel: bordes brillantes
            half fres = pow(1.0 - saturate(abs(dot(N, V))), _FresnelPower);

            // Valor aleatorio por faceta (cuantizando la normal plana)
            float facetRand = Hash31(floor(flatN * 3.0 + 0.5));
            half facetShade = lerp(1.0, 0.4 + facetRand * 0.9, _FacetContrast);

            // Destellos: cada faceta parpadea en un momento distinto
            half flash = sin(_Time.y * _SparkleSpeed + facetRand * 6.2831 + dot(flatN, V) * 4.0);
            flash = pow(saturate(flash), 10.0) * _SparkleIntensity;

            // Pulso suave para llamar la atencion en VR
            half pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

            // Linea de escaneo que sube por el objeto
            half scan = smoothstep(0.85, 1.0, frac(IN.posOSy * _ScanDensity - _Time.y * _ScanSpeed));
            scan *= _ScanIntensity;

            // Color final (cambia a verde cuando la gema esta cerca / valida)
            half3 baseCol = lerp(_BaseColor.rgb, _ActiveColor.rgb, _Active);
            half3 rimCol  = lerp(_RimColor.rgb,  _ActiveColor.rgb, _Active);

            half3 col = baseCol * facetShade
                      + rimCol * fres * _RimIntensity
                      + rimCol * flash * facetRand
                      + rimCol * scan;

            half alpha = _BaseAlpha * facetShade
                       + fres * 0.6
                       + flash * 0.3
                       + scan * 0.3;

            col   *= pulse * intensityMul;
            alpha  = saturate(alpha * pulse * intensityMul);

            return half4(col, alpha);
        }
        ENDHLSL

        // Pass 1: caras traseras (da profundidad al cristal)
        Pass
        {
            Name "CrystalBack"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragBack
            #pragma multi_compile_instancing

            half4 fragBack(Varyings IN) : SV_Target
            {
                return ShadeCrystal(IN, _BackFaceIntensity);
            }
            ENDHLSL
        }

        // Pass 2: caras frontales
        Pass
        {
            Name "CrystalFront"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite Off
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragFront
            #pragma multi_compile_instancing

            half4 fragFront(Varyings IN) : SV_Target
            {
                return ShadeCrystal(IN, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
