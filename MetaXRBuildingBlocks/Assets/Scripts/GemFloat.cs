using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GemFloat : MonoBehaviour
{
    [Header("Giro")]
    [SerializeField] private Vector3 _rotationSpeed = new Vector3(0f, 90f, 0f);
    [Tooltip("Velocidad (grados/s) con la que recupera su rotación inicial al volver.")]
    [SerializeField] private float _rotateBackSpeed = 360f;

    [Header("Flotación")]
    [SerializeField] private float _bobHeight = 0.1f;
    [SerializeField] private float _bobSpeed = 1.5f;

    [Header("Volver a su sitio")]
    [SerializeField] private float _returnRadius = 0.3f;
    [SerializeField] private float _returnSpeed = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Grabbable _grabbable;
    [SerializeField] private SnapInteractor _snapInteractor;

    private Rigidbody _rb;
    private Vector3 _homePosition;
    private Quaternion _homeRotation;
    private Vector3 _basePosition;
    private Quaternion _baseRotation;
    private float _spinTime;
    private float _timeOffset;
    private bool _isFloating;
    private int _selectCount;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_grabbable == null) _grabbable = GetComponent<Grabbable>();
        if (_snapInteractor == null) _snapInteractor = GetComponentInChildren<SnapInteractor>(true);
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
        _homePosition = transform.position;
        _homeRotation = transform.rotation;
        _timeOffset = Random.value * Mathf.PI * 2f;
        StartFloating();
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select) _selectCount++;
        else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            _selectCount = Mathf.Max(0, _selectCount - 1);
    }
    private bool IsHeldByHand()
    {
        bool snapped = _snapInteractor != null && _snapInteractor.HasSelectedInteractable;
        return _selectCount > (snapped ? 1 : 0);
    }

    private void Update()
    {
        bool heldByHand = IsHeldByHand();

        if (_isFloating)
        {
            if (heldByHand) StopFloating();
        }
        else if (!heldByHand &&
                 Vector3.Distance(transform.position, _homePosition) <= _returnRadius)
        {
            StartFloating();
        }

        if (!_isFloating) return;

        _basePosition = Vector3.MoveTowards(_basePosition, _homePosition, _returnSpeed * Time.deltaTime);
        _baseRotation = Quaternion.RotateTowards(_baseRotation, _homeRotation, _rotateBackSpeed * Time.deltaTime);

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

    private void StartFloating()
    {
        _isFloating = true;
        _basePosition = transform.position;
        _baseRotation = transform.rotation;
        _spinTime = 0f;

        if (_snapInteractor != null) _snapInteractor.enabled = false;

        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void StopFloating()
    {
        _isFloating = false;
        if (_snapInteractor != null) _snapInteractor.enabled = true;
        _rb.useGravity = true;
    }
}