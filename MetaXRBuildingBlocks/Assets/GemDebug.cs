using System.Reflection;
using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GemDebug : MonoBehaviour
{
    Rigidbody _rb;
    GemFloat _float;
    SnapInteractor _snap;
    FieldInfo _isFloatingField;
    FieldInfo _homeField;
    float _nextLog;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _float = GetComponent<GemFloat>();
        _snap = GetComponentInChildren<SnapInteractor>(true);

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        _isFloatingField = typeof(GemFloat).GetField("_isFloating", flags);
        _homeField = typeof(GemFloat).GetField("_homePosition", flags);
    }

    void Update()
    {
        if (Time.time < _nextLog) return;
        _nextLog = Time.time + 0.5f;

        bool floating = _float != null && _isFloatingField != null && (bool)_isFloatingField.GetValue(_float);
        Vector3 home = _float != null && _homeField != null ? (Vector3)_homeField.GetValue(_float) : Vector3.zero;

        Debug.Log(
            "[GemDebug] pos=" + transform.position.ToString("F2") +
            " home=" + home.ToString("F2") +
            " distHome=" + Vector3.Distance(transform.position, home).ToString("F2") +
            " floating=" + floating +
            " snapEnabled=" + (_snap != null && _snap.enabled) +
            " snapped=" + (_snap != null && _snap.HasSelectedInteractable) +
            " kinematic=" + _rb.isKinematic);
    }
}