using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsGrabTransformer : MonoBehaviour, ITransformer
{
    [Header("Seguimiento")]
    [SerializeField] private float _maxLinearSpeed = 40f;
    [SerializeField] private float _maxAngularSpeed = 40f;
    [Tooltip("Velocidad maxima con la que el objeto puede salir de las colisiones (evita que salga disparado).")]
    [SerializeField] private float _maxDepenetrationVelocity = 3f;

    [Tooltip("Si el palo se separa mas de esto (m) de la mano, se recoloca de golpe (teletransporte).")]
    [SerializeField] private float _snapDistance = 0.75f;

    [Header("Al soltar")]
    [SerializeField] private bool _gravityWhenReleased = true;
    [Tooltip("Velocidad maxima al soltar. 0 = se suelta sin velocidad (cae).")]
    [SerializeField] private float _maxReleaseSpeed = 3f;

    private IGrabbable _grabbable;
    private Rigidbody _rb;
    private Pose _grabDeltaInLocalSpace;
    private bool _grabbing;
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Vector3 _targetVel;
    private float _lastTargetTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
    }

    public void BeginTransform()
    {
        Pose grabPoint = _grabbable.GrabPoints[0];
        Transform t = _grabbable.Transform;

        _grabDeltaInLocalSpace = new Pose(
            t.InverseTransformVector(grabPoint.position - t.position),
            Quaternion.Inverse(grabPoint.rotation) * t.rotation);

        _targetPos = t.position;
        _targetRot = t.rotation;
        _targetVel = Vector3.zero;
        _lastTargetTime = Time.time;

        _rb.isKinematic = false;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.maxDepenetrationVelocity = _maxDepenetrationVelocity;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _grabbing = true;
    }

    public void UpdateTransform()
    {
        if (!_grabbing) return;

        Pose grabPoint = _grabbable.GrabPoints[0];
        Transform t = _grabbable.Transform;

        _targetRot = grabPoint.rotation * _grabDeltaInLocalSpace.rotation;
        Vector3 newPos = grabPoint.position
                         - _targetRot * Vector3.Scale(_grabDeltaInLocalSpace.position, t.localScale);

        float now = Time.time;
        float dtT = now - _lastTargetTime;
        if (dtT > 1e-4f && dtT < 0.1f)
        {
            Vector3 rawVel = (newPos - _targetPos) / dtT;
            _targetVel = Vector3.Lerp(_targetVel, rawVel, 0.5f);
        }
        _targetPos = newPos;
        _lastTargetTime = now;
    }

    private void FixedUpdate()
    {
        if (!_grabbing) return;

        float dt = Time.fixedDeltaTime;

        float ahead = Mathf.Clamp(Time.time - _lastTargetTime, 0f, 0.05f);
        Vector3 predictedPos = _targetPos + _targetVel * ahead;

        if ((predictedPos - _rb.position).magnitude > _snapDistance)
        {
            _rb.position = predictedPos;
            _rb.rotation = _targetRot;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _targetVel = Vector3.zero;
            return;
        }

        Vector3 vel = (predictedPos - _rb.position) / dt;
        _rb.linearVelocity = Vector3.ClampMagnitude(vel, _maxLinearSpeed);

        Quaternion delta = _targetRot * Quaternion.Inverse(_rb.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        if (float.IsNaN(axis.x) || float.IsInfinity(axis.x) || axis == Vector3.zero)
        {
            _rb.angularVelocity = Vector3.zero;
        }
        else
        {
            Vector3 angVel = axis.normalized * (angle * Mathf.Deg2Rad / dt);
            _rb.angularVelocity = Vector3.ClampMagnitude(angVel, _maxAngularSpeed);
        }
    }

    public void EndTransform()
    {
        if (!_grabbing) return;
        _grabbing = false;

        _rb.useGravity = _gravityWhenReleased;
        _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, _maxReleaseSpeed);
        _rb.angularVelocity = Vector3.ClampMagnitude(_rb.angularVelocity, _maxReleaseSpeed);
        _rb.maxDepenetrationVelocity = 100f;
    }
}