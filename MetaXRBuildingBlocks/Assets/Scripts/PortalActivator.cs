
using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

public class PortalActivator : MonoBehaviour
{
    [Serializable]
    public class WorldEntry
    {
        public string worldId;

        public GameObject world;

        public UnityEvent onOpened;
    }

    [SerializeField] private SnapInteractable _snapInteractable;

    [Header("Mundos")]
    [SerializeField] private WorldEntry[] _worlds;

    [Header("Comun a todos los mundos")]
    [SerializeField] private GameObject[] _cover;
    [SerializeField] private AudioSource _openSound;

    [Header("Gem Snap")]
    [Tooltip("MeshRenderer que se oculta al colocar la gema.")]
    [SerializeField] private MeshRenderer _gemSnapRenderer;

    [Header("Opciones")]
    [SerializeField] private bool _closeWhenGemRemoved = true;

    private WorldEntry _current;

    private void Reset()
    {
        _snapInteractable = GetComponent<SnapInteractable>();
    }

    private void Start()
    {
        CloseAll();

        if (_gemSnapRenderer != null)
            _gemSnapRenderer.enabled = true;
    }

    private void OnEnable()
    {
        if (_snapInteractable == null) return;

        _snapInteractable.WhenSelectingInteractorAdded.Action += OnGemSnapped;
        _snapInteractable.WhenSelectingInteractorRemoved.Action += OnGemRemoved;
    }

    private void OnDisable()
    {
        if (_snapInteractable == null) return;

        _snapInteractable.WhenSelectingInteractorAdded.Action -= OnGemSnapped;
        _snapInteractable.WhenSelectingInteractorRemoved.Action -= OnGemRemoved;
    }

    private void OnGemSnapped(SnapInteractor interactor)
    {
        Gem gem = interactor.GetComponentInParent<Gem>();

        if (gem == null)
        {
            Debug.LogWarning("PortalActivator: la pieza incrustada no tiene componente Gem.", this);
            return;
        }

        WorldEntry entry = FindWorld(gem.WorldId);

        if (entry == null)
        {
            Debug.LogWarning($"PortalActivator: no hay mundo para el WorldId '{gem.WorldId}'.", this);
            return;
        }

        // Ocultar MeshRenderer del GemSnap
        if (_gemSnapRenderer != null)
            _gemSnapRenderer.enabled = false;

        Open(entry);
    }

    private void OnGemRemoved(SnapInteractor interactor)
    {
        // Volver a mostrar MeshRenderer del GemSnap
        if (_gemSnapRenderer != null)
            _gemSnapRenderer.enabled = true;

        if (_closeWhenGemRemoved)
            CloseAll();
    }

    private WorldEntry FindWorld(string id)
    {
        foreach (WorldEntry w in _worlds)
        {
            if (w != null && w.worldId == id)
                return w;
        }

        return null;
    }

    private void Open(WorldEntry entry)
    {
        CloseAll();

        _current = entry;

        if (entry.world != null)
            entry.world.SetActive(true);

        foreach (GameObject c in _cover)
        {
            if (c != null)
                c.SetActive(false);
        }

        if (_openSound != null)
            _openSound.Play();

        entry.onOpened?.Invoke();
    }

    private void CloseAll()
    {
        _current = null;

        foreach (WorldEntry w in _worlds)
        {
            if (w != null && w.world != null)
                w.world.SetActive(false);
        }

        foreach (GameObject c in _cover)
        {
            if (c != null)
                c.SetActive(true);
        }
    }
}