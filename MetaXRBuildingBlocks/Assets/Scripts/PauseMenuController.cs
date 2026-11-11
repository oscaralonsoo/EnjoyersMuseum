using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject menu;
    public Transform head;
    public float distance = 1.5f;
    public float height = 0f;

    [Header("Seguimiento")]
    public bool follow = true;
    public float positionSmoothing = 4f;
    public float rotationSmoothing = 4f;
    public float angleDeadZone = 20f;

    bool open;

    void Start()
    {
        SetOpen(false);
    }

    void Update()
    {
        if (!open || !follow) return;

        Vector3 forward = FlatForward();
        Vector3 targetPos = head.position + forward * distance + Vector3.up * height;

        Vector3 menuDir = menu.transform.position - head.position;
        menuDir.y = 0f;
        float angle = menuDir.sqrMagnitude > 0.001f ? Vector3.Angle(forward, menuDir) : 180f;

        if (angle > angleDeadZone)
        {
            float dt = Time.unscaledDeltaTime;
            menu.transform.position = Vector3.Lerp(menu.transform.position, targetPos, positionSmoothing * dt);
            menu.transform.rotation = Quaternion.Slerp(
                menu.transform.rotation,
                Quaternion.LookRotation(forward),
                rotationSmoothing * dt);
        }
    }

    public void Toggle()
    {
        SetOpen(!open);
    }

    public void Resume()
    {
        SetOpen(false);
    }

    Vector3 FlatForward()
    {
        Vector3 forward = head.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        return forward.normalized;
    }

    void SetOpen(bool value)
    {
        open = value;

        if (open)
        {
            Vector3 forward = FlatForward();
            menu.transform.position = head.position + forward * distance + Vector3.up * height;
            menu.transform.rotation = Quaternion.LookRotation(forward);
        }

        menu.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
        AudioListener.pause = open;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}