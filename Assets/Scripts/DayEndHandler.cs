using UnityEngine;

public class DayEndHandler : MonoBehaviour
{
    public TimeHandler timeHandler;
    private GenreValues startListeners;
    private float startMoney;
    public Cassette[] allCassettes;
    public Cassette[] dailyOffer;
    private RadioStation radioStation;

    private DaySummaryScreen summaryScreen;
    private GameEndScreen endScreen;

    // ------------------------------------------------------------------
    void Start()
    {
        startListeners = new GenreValues
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
        //if no money at the beginning of new day game finishes
        if (radioStation.GetCurrentMoney() <= 0)
        {
            HandleGameFinished();
            return;
        }

        int hipHopDiff = radioStation.currentListeners.hipHop - startListeners.hipHop;
        int discoDiff = radioStation.currentListeners.disco - startListeners.disco;
        int rockDiff = radioStation.currentListeners.rock - startListeners.rock;
        int metalDiff = radioStation.currentListeners.metal - startListeners.metal;
        float moneyDiff = radioStation.GetCurrentMoney() - startMoney;

        Debug.Log($"Day ended! HipHop:{hipHopDiff}, Disco:{discoDiff}, Rock:{rockDiff}, Metal:{metalDiff}, Money:{moneyDiff}");

        // Show day summary — generate next daily offer after player clicks continue
        if (summaryScreen == null)
            summaryScreen = gameObject.AddComponent<DaySummaryScreen>();

        summaryScreen.Show(
            day: timeHandler.CurrentDay,
            finalMoney: radioStation.GetCurrentMoney(),
            moneyDiff: moneyDiff,
            hipHop: radioStation.currentListeners.hipHop,
            hipHopDiff: hipHopDiff,
            disco: radioStation.currentListeners.disco,
            discoDiff: discoDiff,
            rock: radioStation.currentListeners.rock,
            rockDiff: rockDiff,
            metal: radioStation.currentListeners.metal,
            metalDiff: metalDiff,
            onContinueCallback: () =>
            {
                // Update snapshots and generate offer only after player dismisses summary
                startListeners.hipHop = radioStation.currentListeners.hipHop;
                startListeners.disco = radioStation.currentListeners.disco;
                startListeners.rock = radioStation.currentListeners.rock;
                startListeners.metal = radioStation.currentListeners.metal;
                startMoney = radioStation.GetCurrentMoney();

                GenerateDailyOffer();
            }
        );
    }

    void HandleGameFinished()
    {
        int hipHopDiff = radioStation.currentListeners.hipHop - startListeners.hipHop;
        int discoDiff = radioStation.currentListeners.disco - startListeners.disco;
        int rockDiff = radioStation.currentListeners.rock - startListeners.rock;
        int metalDiff = radioStation.currentListeners.metal - startListeners.metal;
        float moneyDiff = radioStation.GetCurrentMoney() - startMoney;

        Debug.Log("[DayEndHandler] Game finished — showing end screen.");

        if (endScreen == null)
            endScreen = gameObject.AddComponent<GameEndScreen>();

        int endGameCause = 0;

        if (radioStation.GetCurrentMoney() <= 0)
            endGameCause = 1;
        else if (radioStation.GetTotalListeners() < 1000)
            endGameCause = 2;

        endScreen.Show(
            endGameCause: endGameCause,
            totalDays: timeHandler.CurrentDay,
            finalMoney: radioStation.GetCurrentMoney(),
            moneyDiff: moneyDiff,
            hipHop: radioStation.currentListeners.hipHop,
            hipHopDiff: hipHopDiff,
            disco: radioStation.currentListeners.disco,
            discoDiff: discoDiff,
            rock: radioStation.currentListeners.rock,
            rockDiff: rockDiff,
            metal: radioStation.currentListeners.metal,
            metalDiff: metalDiff
        );
    }

    // ------------------------------------------------------------------
    public void Initialize(RadioStation rs, TimeHandler th)
    {
        radioStation = rs;
        timeHandler = th;

        timeHandler.OnDayStarted += HandleDayStart;
        timeHandler.OnGameFinished += HandleGameFinished;

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
        for (int i = 0; i < usedIndexes.Length; i++)
            usedIndexes[i] = i;

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, usedIndexes.Length - i);
            dailyOffer[i] = allCassettes[usedIndexes[randomIndex]];

            int temp = usedIndexes[randomIndex];
            usedIndexes[randomIndex] = usedIndexes[usedIndexes.Length - 1 - i];
            usedIndexes[usedIndexes.Length - 1 - i] = temp;
        }

        foreach (var c in dailyOffer)
            Debug.Log("Offered cassette: " + c.name);
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
