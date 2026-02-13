using UnityEngine;

[CreateAssetMenu(fileName = "NowaKaseta", menuName = "Radio/Kaseta")]

/// <summary>
/// Klasa przechowuj¹ca dane kasety
/// </summary
public class Cassette : PlayableContent {

    public AudioClip song;
    public GenreValues listenerGrowthPrecentage;  //Procentowa wartoœæ wzrostu s³uchaczy 100 -> 100% 50 -> 50%
    [SerializeField] private int timesUsedInDay;

    // Mo¿esz tu dodaæ metody pomocnicze
    public void Play(AudioSource source)
    {
        source.clip = song;
        source.Play();
    }

    public override void ApplyEffect(Radio radio)
    {
        throw new System.NotImplementedException();
    }
}