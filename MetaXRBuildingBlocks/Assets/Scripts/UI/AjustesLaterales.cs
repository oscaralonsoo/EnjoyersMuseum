using UnityEngine;

public class AjustesLaterales : MonoBehaviour
{
    public GameObject[] laterales;

    void OnEnable() { foreach (var g in laterales) if (g) g.SetActive(true); }
    void OnDisable() { foreach (var g in laterales) if (g) g.SetActive(false); }
}