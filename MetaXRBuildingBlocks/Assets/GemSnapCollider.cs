using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GemSnapCollider : MonoBehaviour
{
    [Header("Colocación en el hueco")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _rotateSpeed = 300f;

    private Rigidbody _rb;
    private SnapInteractor _snapInteractor;
    private Collider[] _solidColliders;
    private bool _wasSnapped;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _snapInteractor = GetComponentInChildren<SnapInteractor>(true);


        var all = GetComponentsInChildren<Collider>(true);
        var list = new System.Collections.Generic.List<Collider>();
        foreach (var c in all)
            if (!c.isTrigger) list.Add(c);
        _solidColliders = list.ToArray();
    }

    private void LateUpdate()
    {
        bool snapped = _snapInteractor != null && _snapInteractor.HasSelectedInteractable;

        foreach (var c in _solidColliders)
            if (c != null && c.enabled == snapped) c.enabled = !snapped;

        if (snapped)
        {
            if (!_rb.isKinematic) _rb.isKinematic = true;

            Transform target = _snapInteractor.SelectedInteractable.transform;
            transform.position = Vector3.MoveTowards(
                transform.position, target.position, _moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target.rotation, _rotateSpeed * Time.deltaTime);
        }
        else if (_wasSnapped)
        {
            _rb.isKinematic = false;
        }

        _wasSnapped = snapped;
    }
}