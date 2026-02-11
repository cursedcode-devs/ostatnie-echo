using UnityEngine;

[CreateAssetMenu(fileName = "NowaKaseta", menuName = "Muzyka/Kaseta")]
public class Cassette : ScriptableObject
{
    [SerializeField] private string cassetteName; // Zmieniono nazwê, bo 'name' jest zarezerwowane w SO
    [SerializeField] private int timesUsedInDay;

    [Header("Gains")]
    [SerializeField] private int gainListenersHipHop;
    [SerializeField] private int gainListenersDisco;
    [SerializeField] private int gainListenersRock;
    [SerializeField] private int gainListenersMetal;

    [SerializeField] private AudioClip musicClip;

    // Mo¿esz tu dodaæ metody pomocnicze
    public void Play(AudioSource source)
    {
        source.clip = musicClip;
        source.Play();
    }
}