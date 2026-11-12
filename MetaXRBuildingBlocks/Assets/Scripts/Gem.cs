using UnityEngine;

public class Gem : MonoBehaviour
{
    [SerializeField] private string _worldId = "mundo1";
    public string WorldId => _worldId;
}
