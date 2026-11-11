Shader "Custom/PortalStencilWrite"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        _Ref ("Stencil Ref", Float) = 1
        _BackColor ("Color de fondo del portal", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+100" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float _Cull;
            float _Ref;
            half4 _BackColor;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings vert(Attributes i)
        {
            Varyings o;
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "StencilMark"
            Cull [_Cull]
            ZTest LEqual
            ZWrite Off
            ColorMask 0
            Stencil { Ref [_Ref] Comp Always Pass Replace }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ClearInside"
            Cull [_Cull]
            ZTest Always
            ZWrite On
            Stencil { Ref [_Ref] Comp Equal Pass Keep }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct FragOut
            {
                half4 color : SV_Target;
                float depth : SV_Depth;
            };

            FragOut frag(Varyings i)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                FragOut o;
                o.color = _BackColor;
                o.depth = UNITY_RAW_FAR_CLIP_VALUE;
                return o;
            }
            ENDHLSL
        }
    }
}