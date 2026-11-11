using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject menu;
    public Transform head;
    public float distance = 1.0f;
    public float height = 0f;

    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelAjustes;
    public GameObject panelControles;

    [Header("Paneles de la V)")]
    public GameObject[] panelesAjustes;

    [Header("Seguimiento")]
    public bool follow = true;
    public float positionSmoothing = 4f;
    public float rotationSmoothing = 4f;
    public float angleDeadZone = 15f;

    bool open;
    Coroutine closeRoutine;

    void Start()
    {
        SetOpen(false, true);
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

    public void Toggle() { SetOpen(!open); }
    public void Resume() { SetOpen(false); }
    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowMain() { ShowPanel(true, false, false); }
    public void ShowSettings() { ShowPanel(false, true, false); }
    public void ShowControls() { ShowPanel(false, false, true); }

    void ShowPanel(bool principal, bool ajustes, bool controles)
    {
        UIAnimator.SetActive(panelPrincipal, principal);
        UIAnimator.SetActive(panelAjustes, ajustes);
        UIAnimator.SetActive(panelControles, controles);

        if (panelesAjustes != null)
            foreach (var p in panelesAjustes)
                UIAnimator.SetActive(p, ajustes);
    }

    Vector3 FlatForward()
    {
        Vector3 forward = head.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        return forward.normalized;
    }

    void SetOpen(bool value, bool instant = false)
    {
        open = value;

        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        if (open)
        {
            ShowMain();

            Vector3 forward = FlatForward();
            menu.transform.position = head.position + forward * distance + Vector3.up * height;
            menu.transform.rotation = Quaternion.LookRotation(forward);

            if (instant) menu.SetActive(true);
            else UIAnimator.SetActive(menu, true);
        }
        else
        {
            if (instant) menu.SetActive(false);
            else Close();
        }

        Time.timeScale = open ? 0f : 1f;
        AudioListener.pause = open;
    }

    void Close()
    {
        ShowPanel(false, false, false);

        if (menu.GetComponent<UIAnimator>() != null)
        {
            UIAnimator.SetActive(menu, false);
            return;
        }

        closeRoutine = StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        float wait = Mathf.Max(
            AnimTime(panelPrincipal),
            AnimTime(panelAjustes),
            AnimTime(panelControles));

        if (panelesAjustes != null)
            foreach (var p in panelesAjustes)
                wait = Mathf.Max(wait, AnimTime(p));

        yield return new WaitForSecondsRealtime(wait);

        closeRoutine = null;
        if (!open) menu.SetActive(false);
    }

    float AnimTime(GameObject go)
    {
        if (go == null) return 0f;
        var a = go.GetComponent<UIAnimator>();
        return a != null ? a.duration : 0f;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}