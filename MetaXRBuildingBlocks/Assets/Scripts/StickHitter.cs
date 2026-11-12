using UnityEngine;

/// <summary>
/// Pon este script en el Stick. Mide la velocidad real del palo (lineal + angular) y, al chocar con el
/// balon, le aplica el golpe directamente. Asi el golpe no depende de como PhysX trate a un
/// Rigidbody kinematic movido por Transform (que casi no transmite velocidad).
/// Requiere que el balon tenga el tag "Ball" y un Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class StickHitter : MonoBehaviour
{
    [Header("Balon")]
    [SerializeField] private string _ballTag = "Ball";

    [Header("Golpe")]
    [Tooltip("Rebote. 0 = el balon sale a la velocidad del palo, 1 = rebote perfecto (sale al doble).")]
    [SerializeField, Range(0f, 1f)] private float _restitution = 0.6f;
    [Tooltip("Multiplicador general de potencia.")]
    [SerializeField] private float _hitMultiplier = 1.2f;
    [Tooltip("Cuanto arrastra el palo al balon en la direccion tangencial (efecto de 'cuchara').")]
    [SerializeField, Range(0f, 1f)] private float _tangentialCarry = 0.3f;
    [Tooltip("Velocidad minima del palo hacia el balon para considerarlo golpe (m/s).")]
    [SerializeField] private float _minHitSpeed = 0.3f;
    [SerializeField] private float _maxBallSpeed = 30f;

    [Header("Medida de velocidad")]
    [Tooltip("0-1. Mas alto = reacciona mas rapido pero mas ruido. Mas bajo = mas suave.")]
    [SerializeField, Range(0.05f, 1f)] private float _smoothing = 0.5f;
    [Tooltip("Por encima de esta velocidad se considera teletransporte y se ignora (m/s).")]
    [SerializeField] private float _teleportSpeed = 40f;

    private Vector3 _velocity;
    private Vector3 _angularVelocity; // rad/s, mundo
    private Vector3 _lastPos;
    private Quaternion _lastRot;
    private bool _initialized;

    private void OnEnable()
    {
        _initialized = false;
        _velocity = Vector3.zero;
        _angularVelocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        if (!_initialized)
        {
            _lastPos = pos;
            _lastRot = rot;
            _initialized = true;
            return;
        }

        Vector3 rawVel = (pos - _lastPos) / dt;

        Quaternion delta = rot * Quaternion.Inverse(_lastRot);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        Vector3 rawAng = Vector3.zero;
        if (!float.IsNaN(axis.x) && !float.IsInfinity(axis.x) && axis != Vector3.zero)
            rawAng = axis.normalized * (angle * Mathf.Deg2Rad / dt);

        if (rawVel.magnitude > _teleportSpeed)
        {
            rawVel = Vector3.zero;
            rawAng = Vector3.zero;
        }

        _velocity = Vector3.Lerp(_velocity, rawVel, _smoothing);
        _angularVelocity = Vector3.Lerp(_angularVelocity, rawAng, _smoothing);

        _lastPos = pos;
        _lastRot = rot;
    }

    private void OnCollisionEnter(Collision collision) => TryHit(collision);
    private void OnCollisionStay(Collision collision) => TryHit(collision);

    private void TryHit(Collision collision)
    {
        if (!collision.collider.CompareTag(_ballTag)) return;

        Rigidbody ball = collision.rigidbody;
        if (ball == null) return;

        ContactPoint cp = collision.GetContact(0);
        Vector3 p = cp.point;

        // Velocidad del punto del palo que golpea (lineal + giro).
        Vector3 pointVel = _velocity + Vector3.Cross(_angularVelocity, p - transform.position);

        // Direccion del golpe: del punto de contacto al centro del balon.
        Vector3 n = ball.worldCenterOfMass - p;
        if (n.sqrMagnitude < 1e-6f) n = -cp.normal;
        n.Normalize();

        float stickAlongN = Vector3.Dot(pointVel, n);
        if (stickAlongN < _minHitSpeed) return;

        float desired = stickAlongN * (1f + _restitution) * _hitMultiplier;
        Vector3 ballVel = ball.linearVelocity;
        float current = Vector3.Dot(ballVel, n);
        if (current >= desired) return; // idempotente: no suma de mas si ya sale suficientemente rapido

        ballVel += n * (desired - current);

        // Arrastre tangencial
        Vector3 stickTan = Vector3.ProjectOnPlane(pointVel, n);
        Vector3 ballTan = Vector3.ProjectOnPlane(ballVel, n);
        ballVel += (stickTan - ballTan) * _tangentialCarry;

        ball.linearVelocity = Vector3.ClampMagnitude(ballVel, _maxBallSpeed);
    }
}
