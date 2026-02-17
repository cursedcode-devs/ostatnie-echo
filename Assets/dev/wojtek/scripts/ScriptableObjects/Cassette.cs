using UnityEngine;

[CreateAssetMenu(fileName = "NowaKaseta", menuName = "Radio/Kaseta")]

/// <summary>
/// Klasa przechowuj¹ca dane kasety
/// </summary
public class Cassette : PlayableContent 
{
    public GenreValues listenerGrowthPrecentage;  //Procentowa wartoœæ wzrostu s³uchaczy 100 -> 100% 50 -> 50%
    [SerializeField] private int timesUsedInDay;

    public override void ApplyEffect(RadioStation radio)
    {
        radio.AddListeners(listenerGrowthPrecentage);
    }
}