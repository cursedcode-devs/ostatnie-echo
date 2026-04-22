using UnityEngine;

[CreateAssetMenu(fileName = "NowaKaseta", menuName = "Radio/Kaseta")]

/// <summary>
/// Klasa przechowuj�ca dane kasety
/// </summary>
public class Cassette : PlayableContent 
{
    public GenreValues listenerGrowthPrecentage;  //Procentowa warto�� wzrostu s�uchaczy 100 -> 100% 50 -> 50%

    public override void ApplyEffect(RadioStation radio)
    {
        timesUsedInDay++;
        radio.AddCassette(listenerGrowthPrecentage);
    }
}