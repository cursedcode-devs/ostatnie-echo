using UnityEngine;

//[CreateAssetMenu(fileName = "PlayableContent", menuName = "Scriptable Objects/PlayableContent")]
//
// Klasa z kt�rej dziedziczy ka�dy Playable item np. kaseta, reklama
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
    [SerializeField] public float price;


    public string GetName()
    { return itemName; }

    public string GetAuthor()
    { return author; }

    public void ResetLastValues()
    {
        lastCassetteValues = cassetteValues;
    }

    public void SetLastValues(int hipHop, int disco, int rock, int pop)
    {
        lastCassetteValues.rock = rock;
        lastCassetteValues.hipHop = hipHop;
        lastCassetteValues.disco = disco;
        lastCassetteValues.pop = pop;
    }

    public GenreValues GetLastValues()
    {
        return lastCassetteValues;
    }

    public int GetHipHop()
    {
        return cassetteValues.hipHop;
    }

    public int GetRock()
    {
        return cassetteValues.rock;
    }

    public int GetPop()
    {
        return cassetteValues.pop;
    }

    public int GetDisco()
    {
        return cassetteValues.disco;
    }

    public new CassetteTypes GetType()
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
