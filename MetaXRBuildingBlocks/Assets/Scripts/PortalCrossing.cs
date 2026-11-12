using System.Collections.Generic;
using UnityEngine;

public class PortalCrossing : MonoBehaviour
{
    public Transform portal;
    public Renderer portalRenderer;
    public Transform head;
    public GameObject fullStencil;

    [Header("Objetos que se desactivan al cruzar el portal")]
    [Tooltip("Se desactivan al entrar y se reactivan al volver a cruzar.")]
    public List<GameObject> disableOnCross = new List<GameObject>();

    bool inside;
    float prevZ;
    bool hasPrev;

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

        // Al entrar se desactivan, al salir se reactivan
        for (int i = 0; i < disableOnCross.Count; i++)
        {
            if (disableOnCross[i] != null)
                disableOnCross[i].SetActive(!inside);
        }

        Material m = portalRenderer.material;
        m.SetFloat("_Ref", inside ? 0f : 1f);
        m.SetFloat("_Cull", inside ? 0f : 2f);
    }
}