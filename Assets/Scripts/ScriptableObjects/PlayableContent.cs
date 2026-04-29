using UnityEngine;

//[CreateAssetMenu(fileName = "PlayableContent", menuName = "Scriptable Objects/PlayableContent")]
//
// Klasa z której dziedziczy ka¿dy Playable item np. kaseta, reklama
//
public abstract class PlayableContent : ScriptableObject
{
    public AudioClip audio;
    [SerializeField] private string itemName;
    [SerializeField] private string author;
    [SerializeField] private GameObject physicalPrefab;
    [SerializeField] protected int timesUsedInDay = 0;
    [SerializeField] protected GenreValues cassetteValues;
    [SerializeField] protected CassetteTypes type;
    [SerializeField] private GenreValues lastCassetteValues;

    public void ResetLastValues()
    {
        lastCassetteValues = cassetteValues;
    }

    public void SetLastValues(int hipHop, int disco, int rock, int metal)
    {
        lastCassetteValues.rock = rock;
        lastCassetteValues.hipHop = hipHop;
        lastCassetteValues.disco = disco;
        lastCassetteValues.metal = metal;
    }

    public GenreValues GetLastValues()
    {
        return lastCassetteValues;
    }

    public CassetteTypes GetType()
    {
        return type;
    }
    public GenreValues GetCassetteValues()
    {
        return cassetteValues;
    }

    public int GetTimesUsed()
    {
        return timesUsedInDay;
    }

    public void IncreaseTimesUsed()
    {
        timesUsedInDay++;
    }

    public void SetCassetteValues(GenreValues genreValues)
    {
        cassetteValues = genreValues;
    }

    public void Play(ref AudioSource source)
    {
        source.clip = audio;
        source.Play();
    }

    public void ResetTimesUsed()
    {
        timesUsedInDay = 0;
    }
}
