using UnityEngine;

//[CreateAssetMenu(fileName = "PlayableContent", menuName = "Scriptable Objects/PlayableContent")]
public abstract class PlayableContent : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private GameObject physicalPrefab;

    public abstract void ApplyEffect(Radio radio);
}
