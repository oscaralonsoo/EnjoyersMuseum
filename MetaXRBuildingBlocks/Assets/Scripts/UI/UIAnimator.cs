using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class UIAnimator : MonoBehaviour
{
    [Header("Efectos")]
    public bool fade = true;
    public bool scale = true;
    public bool slide = false;

    [Header("Tiempos")]
    public float duration = 0.25f;
    public float delay = 0f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Valores")]
    [Range(0f, 1f)] public float startScale = 0.85f;
    public Vector2 slideOffset = new Vector2(0f, -30f);

    CanvasGroup group;
    RectTransform rect;
    Vector3 baseScale;
    Vector2 basePos;
    float progress;
    Coroutine routine;

    public static void SetActive(GameObject go, bool active)
    {
        if (go == null) return;

        var anim = go.GetComponent<UIAnimator>();
        if (anim == null) { go.SetActive(active); return; }

        if (active) anim.Show();
        else anim.Hide();
    }

    public void Show()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        else if (gameObject.activeInHierarchy) Play(true);
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy) { gameObject.SetActive(false); return; }
        Play(false);
    }

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        rect = transform as RectTransform;
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        if (rect != null) basePos = rect.anchoredPosition;
        progress = 0f;
        Play(true);
    }

    void OnDisable()
    {
        routine = null;
        progress = 1f;
        Apply(1f);
        group.interactable = true;
    }

    void Play(bool show)
    {
        if (!gameObject.activeInHierarchy) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run(show));
    }

    IEnumerator Run(bool show)
    {
        float target = show ? 1f : 0f;
        group.interactable = show;

        if (show)
        {
            Apply(curve.Evaluate(progress));
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        }

        float start = progress;
        float dur = Mathf.Max(0.0001f, duration * Mathf.Abs(target - start));
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            progress = Mathf.Lerp(start, target, t / dur);
            Apply(curve.Evaluate(progress));
            yield return null;
        }

        progress = target;
        Apply(curve.Evaluate(progress));
        routine = null;

        if (show) group.interactable = true;
        else gameObject.SetActive(false);
    }

    void Apply(float e)
    {
        if (fade) group.alpha = e;
        else group.alpha = 1f;

        transform.localScale = scale ? baseScale * Mathf.Lerp(startScale, 1f, e) : baseScale;

        if (slide && rect != null)
            rect.anchoredPosition = basePos + Vector2.Lerp(slideOffset, Vector2.zero, e);
    }
}