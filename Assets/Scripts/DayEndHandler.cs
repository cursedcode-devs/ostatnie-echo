using UnityEngine;

public class DayEndHandler : MonoBehaviour
{
    public TimeHandler timeHandler;
    private GenreValues startListeners;
    private float startMoney;
    public Cassette[] allCassettes;
    public Cassette[] dailyOffer;
    private RadioStation radioStation;

    /*statystyki
    rachunki - wraz z statami z automatu odejmuje przed zakupami
    ulepszenia - do zakupu
    kasety - do zakupu
    reklamy - do wyboru*/

    void Start()
    {
        startListeners= new GenreValues
        {
            hipHop = radioStation.currentListeners.hipHop,
            rock = radioStation.currentListeners.rock,
            metal = radioStation.currentListeners.metal,
            disco = radioStation.currentListeners.disco
        };
        startMoney = radioStation.GetCurrentMoney();
    }
    void HandleDayStart()
    {
        int hipHopDiff = radioStation.currentListeners.hipHop - startListeners.hipHop;
        int discoDiff = radioStation.currentListeners.disco - startListeners.disco;
        int rockDiff = radioStation.currentListeners.rock - startListeners.rock;
        int metalDiff = radioStation.currentListeners.metal - startListeners.metal;
        float moneyDiff = radioStation.GetCurrentMoney() - startMoney;

        Debug.Log($"Day ended! Listeners change: HipHop:{hipHopDiff}, Disco:{discoDiff}, Rock:{rockDiff}, Metal:{metalDiff}");
        Debug.Log($"Revenue change: {moneyDiff}");

        startListeners.hipHop = radioStation.currentListeners.hipHop;
        startListeners.disco = radioStation.currentListeners.disco;
        startListeners.rock = radioStation.currentListeners.rock;
        startListeners.metal = radioStation.currentListeners.metal;
        startMoney = radioStation.GetCurrentMoney();

        GenerateDailyOffer();
    }

    public void Initialize(RadioStation rs, TimeHandler th)
    {
        radioStation = rs;
        timeHandler = th;

        timeHandler.OnDayStarted += HandleDayStart;

        startListeners = new GenreValues
        {
            hipHop = radioStation.currentListeners.hipHop,
            disco = radioStation.currentListeners.disco,
            rock = radioStation.currentListeners.rock,
            metal = radioStation.currentListeners.metal
        };
        startMoney = radioStation.GetCurrentMoney();
        dailyOffer = new Cassette[3];
    }

    void GenerateDailyOffer()
    {
        if (allCassettes.Length < 3)
        {
            Debug.Log("Not enough cassettes to generate daily offer!");
            return;
        }
        int[] usedIndexes = new int[allCassettes.Length];
        for (int i=0; i<usedIndexes.Length; i++)
        {
            usedIndexes[i] = i;
        }
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, usedIndexes.Length - i);
            dailyOffer[i] = allCassettes[usedIndexes[randomIndex]];

            int temp = usedIndexes[randomIndex];
            usedIndexes[randomIndex] = usedIndexes[usedIndexes.Length - 1 - i];
            usedIndexes[usedIndexes.Length - 1 - i] = temp;
        }
        foreach (var c in dailyOffer)
        {
            Debug.Log("Offered cassette: " + c.name);
        }
    }

    public void AddCassetteToOffer(Cassette cassette)
    {
        var newOffer = new Cassette[dailyOffer.Length + 1];
        newOffer[0] = cassette;
        for (int i = 0; i < dailyOffer.Length; i++)
            newOffer[i + 1] = dailyOffer[i];
        dailyOffer = newOffer;
        Debug.Log($"[DayEndHandler] Dodano '{cassette.name}' do oferty.");
    }

}
