using UnityEngine;

//[CreateAssetMenu(fileName = "PlayableContent", menuName = "Scriptable Objects/PlayableContent")]
//
// Klasa z której dziedziczy ka¿dy Playable item np. kaseta, reklama
//
public abstract class PlayableContent : ScriptableObject
{
    public AudioClip audio;
    [SerializeField] private string itemName;
    [SerializeField] private GameObject physicalPrefab;


    public void Play(ref AudioSource source)
    {
        source.clip = audio;
        source.Play();
    }

    public abstract void ApplyEffect(RadioStation radio);
}
