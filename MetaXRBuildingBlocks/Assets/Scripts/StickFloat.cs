using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickFloat : MonoBehaviour
{
    [Header("Giro")]
    [SerializeField] private Vector3 _rotationSpeed = new Vector3(0f, 90f, 0f);

    [Header("Flotación")]
    [SerializeField] private float _bobHeight = 0.1f;
    [SerializeField] private float _bobSpeed = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Grabbable _grabbable;

    private Rigidbody _rb;
    private Vector3 _basePosition;
    private Quaternion _baseRotation;
    private float _spinTime;
    private float _timeOffset;
    private bool _isFloating;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_grabbable == null) _grabbable = GetComponent<Grabbable>();
    }

    private void OnEnable()
    {
        if (_grabbable != null)
            _grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnDisable()
    {
        if (_grabbable != null)
            _grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void Start()
    {
        _basePosition = transform.position;
        _baseRotation = transform.rotation;
        _timeOffset = Random.value * Mathf.PI * 2f;
        _isFloating = true;

        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select && _isFloating)
            StopFloating();
    }

    private void Update()
    {
        if (!_isFloating) return;

        _spinTime += Time.deltaTime;
        float y = Mathf.Sin(Time.time * _bobSpeed * Mathf.PI * 2f + _timeOffset) * _bobHeight;

        transform.SetPositionAndRotation(
            _basePosition + Vector3.up * y,
            Quaternion.Euler(_rotationSpeed * _spinTime) * _baseRotation);
    }

    private void FixedUpdate()
    {
        if (!_isFloating) return;
        _rb.useGravity = false;
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void StopFloating()
    {
        _isFloating = false;
        _rb.useGravity = true;
    }
}