using System.Collections.Generic;
using UnityEngine;

public class PortalCrossing : MonoBehaviour
{
    public Transform portal;
    public Renderer portalRenderer;
    public Transform head;
    public GameObject fullStencil;

    [Header("Gema en uso")]
    public PortalActivator activator;

    [Header("Objetos que se desactivan al cruzar el portal")]
    public List<GameObject> disableOnCross = new List<GameObject>();

    bool inside;
    float prevZ;
    bool hasPrev;

    readonly List<GameObject> _hiddenNested = new List<GameObject>();

    void Update()
    {
        Vector3 local = portal.InverseTransformPoint(head.position);

        if (hasPrev && Mathf.Abs(local.x) <= 0.5f && Mathf.Abs(local.y) <= 0.5f)
        {
            if (!inside && prevZ < 0f && local.z >= 0f) SetInside(true);
            else if (inside && prevZ > 0f && local.z <= 0f) SetInside(false);
        }

        prevZ = local.z;
        hasPrev = true;
    }

    void SetInside(bool value)
    {
        inside = value;
        if (fullStencil != null) fullStencil.SetActive(inside);

        if (inside) HideAll();
        else ShowAll();

        Material m = portalRenderer.material;
        m.SetFloat("_Ref", inside ? 0f : 1f);
        m.SetFloat("_Cull", inside ? 0f : 2f);
    }

    void HideAll()
    {
        Transform gem = (activator != null && activator.CurrentGem != null)
            ? activator.CurrentGem.transform
            : null;

        for (int i = 0; i < disableOnCross.Count; i++)
        {
            GameObject go = disableOnCross[i];
            if (go == null) continue;

            if (gem != null && gem.IsChildOf(go.transform))
            {
                if (go.transform != gem) HideAllExcept(go.transform, gem);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }
    void HideAllExcept(Transform parent, Transform gem)
    {
        foreach (Transform child in parent)
        {
            if (child == gem) continue;

            if (gem.IsChildOf(child))
            {
                HideAllExcept(child, gem);
                continue;
            }

            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                _hiddenNested.Add(child.gameObject);
            }
        }
    }

    void ShowAll()
    {
        for (int i = 0; i < disableOnCross.Count; i++)
            if (disableOnCross[i] != null) disableOnCross[i].SetActive(true);

        for (int i = 0; i < _hiddenNested.Count; i++)
            if (_hiddenNested[i] != null) _hiddenNested[i].SetActive(true);

        _hiddenNested.Clear();
    }
}