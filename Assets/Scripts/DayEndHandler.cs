using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DayEndHandler : MonoBehaviour
{
    public GameManager gameManager;
    public TimeHandler timeHandler;
    public Canvas ShopUI;
    public Canvas ShopButton;
    private GenreValues startListeners;
    private float startMoney;
    public Cassette[] allCassettes;
    public Ad[] allAds;
    public Cassette[] dailyOffer;
    private RadioStation radioStation;

    private GameEndScreen endScreen;
    public FMODUnity.EventReference buyingSound;
    public FMODUnity.EventReference yawnSound;
    public float[] kawalerka_fees;
    public float[] jedzenie_fees;
    public float[] studia_fees;
    private float kawalerka_fee;
    private float jedzenie_fee;
    private float studia_fee;
    
    private bool gameFinished = false;

    public static DayEndHandler Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShopUI.gameObject.SetActive(false);
        ShopButton.gameObject.SetActive(false);
        startListeners = new GenreValues
        {
            hipHop = radioStation.currentListeners.hipHop,
            rock = radioStation.currentListeners.rock,
            pop = radioStation.currentListeners.pop,
            disco = radioStation.currentListeners.disco
        };
        startMoney = radioStation.GetCurrentMoney();

        foreach (var cassette in allCassettes)
        {
            cassette.ResetTimesUsed();
            cassette.ResetLastValues();
        }
        foreach (var ad in allAds)
        {
            ad.ResetTimesUsed();
            ad.ResetLastValues();
        }
    }

    void HandleDayStart()
    {   
        FMODUnity.RuntimeManager.PlayOneShot(yawnSound, this.transform.position);

        if (timeHandler.getDay() > 1)
        {
            int index = timeHandler.getDay() - 2;
            kawalerka_fee = index < kawalerka_fees.Length ? kawalerka_fees[index] : 0f;
            jedzenie_fee = index < jedzenie_fees.Length ? jedzenie_fees[index] : 0f;
            studia_fee = index < studia_fees.Length ? studia_fees[index] : 0f;
        }
        
        gameManager.SetInputEnabled(false);
        radioStation.SetCurrentMoney(radioStation.GetCurrentMoney() - kawalerka_fee - jedzenie_fee - studia_fee);


        foreach (var cassette in allCassettes)
        {
            cassette.ResetTimesUsed();
            cassette.ResetLastValues();
        }
        foreach (var ad in allAds)
        {
            ad.ResetTimesUsed();
            ad.ResetLastValues();
        }

        radioStation.SetDailyListenersModifier(0f, 0f, 0f, 0f);
        radioStation.SetDailyRevenueModifier(0f, 0f, 0f, 0f);

        float adsPenalty = 0f;
        List<AdContractManager.UnplayedAdPenalty> unplayedPenalties = new List<AdContractManager.UnplayedAdPenalty>();
        var adManager = FindFirstObjectByType<AdContractManager>();
        if (adManager != null)
        {
            adsPenalty = adManager.CalculateAndApplyPenalties();
            unplayedPenalties = new List<AdContractManager.UnplayedAdPenalty>(adManager.lastDayPenalties);
        }

        int hipHopDiff = radioStation.currentListeners.hipHop - startListeners.hipHop;
        int discoDiff = radioStation.currentListeners.disco - startListeners.disco;
        int rockDiff = radioStation.currentListeners.rock - startListeners.rock;
        int popDiff = radioStation.currentListeners.pop - startListeners.pop;
        float moneyDiff = radioStation.GetCurrentMoney() - startMoney;

        Debug.Log($"Day ended! HipHop:{hipHopDiff}, Disco:{discoDiff}, Rock:{rockDiff}, Metal:{popDiff}, Money:{moneyDiff}, AdsPenalty:{adsPenalty}");

        DaySummaryData.Day = timeHandler.CurrentDay;
        DaySummaryData.RentFee = kawalerka_fee;
        DaySummaryData.FoodFee = jedzenie_fee;
        DaySummaryData.StudiesFee = studia_fee;
        DaySummaryData.FinalMoney = radioStation.GetCurrentMoney();
        DaySummaryData.MoneyDiff = moneyDiff;
        
        DaySummaryData.AdsPenalty = adsPenalty;
        DaySummaryData.UnplayedPenalties = unplayedPenalties;

        DaySummaryData.HipHop = radioStation.currentListeners.hipHop;
        DaySummaryData.HipHopDiff = hipHopDiff;
        DaySummaryData.Disco = radioStation.currentListeners.disco;
        DaySummaryData.DiscoDiff = discoDiff;
        DaySummaryData.Rock = radioStation.currentListeners.rock;
        DaySummaryData.RockDiff = rockDiff;
        DaySummaryData.Pop = radioStation.currentListeners.pop;
        DaySummaryData.PopDiff = popDiff;

        Camera mainCam = gameManager.mainCamera;

        DaySummaryData.OnSummaryClosed = () =>
        {
            if (gameFinished || radioStation.GetCurrentMoney() <= 0)
            {
                if (mainCam != null)
                    mainCam.gameObject.SetActive(true);
                
                HandleGameFinished();
                return;
            }

            startListeners.hipHop = radioStation.currentListeners.hipHop;
            startListeners.disco = radioStation.currentListeners.disco;
            startListeners.rock = radioStation.currentListeners.rock;
            startListeners.pop = radioStation.currentListeners.pop;
            startMoney = radioStation.GetCurrentMoney();

            if (mainCam != null)
                mainCam.gameObject.SetActive(true);

            gameManager.SetInputEnabled(true);
            if (timeHandler.getDay() >= 2 && ShopButton != null)
            {
                ShopButton.gameObject.SetActive(true);
            }
        };

        if (mainCam != null)
            mainCam.gameObject.SetActive(false);

        SceneManager.LoadScene("DaySummaryScene", LoadSceneMode.Additive);
    }

    void HandleGameFinished()
    {
        int hipHopDiff = radioStation.currentListeners.hipHop - startListeners.hipHop;
        int discoDiff = radioStation.currentListeners.disco - startListeners.disco;
        int rockDiff = radioStation.currentListeners.rock - startListeners.rock;
        int popDiff = radioStation.currentListeners.pop - startListeners.pop;
        float moneyDiff = radioStation.GetCurrentMoney() - startMoney;

        Debug.Log("[DayEndHandler] Game finished - showing end screen.");

        if (endScreen == null)
            endScreen = gameObject.AddComponent<GameEndScreen>();

        int endGameCause = 0;

        if (radioStation.GetCurrentMoney() <= 0)
            endGameCause = 1;
        else if (radioStation.GetTotalListeners() < 55)
            endGameCause = 2;

        // Każdy koniec gry -> narracyjne zakończenie. Wariant (pełne telegazety + "Gratulacje"
        // vs sam licznik + "Nie osiągnąłeś rozgłosu. Spróbuj jeszcze raz.") zależy od progu
        // słuchaczy i jest rozstrzygany w EndingSceneManager. Stary ekran statystyk niżej =
        // tylko fallback, gdy w scenie brakuje EndingSceneManager.
        EndingData.HostSurvives     = gameManager.GetEndingOutcome(EndingMeter.Host);
        EndingData.ListenersUnite   = gameManager.GetEndingOutcome(EndingMeter.Listener);
        EndingData.GovernmentSignal = gameManager.GetEndingOutcome(EndingMeter.Government);
        EndingData.FinalListeners   = radioStation.GetTotalListeners();

        var ending = FindFirstObjectByType<EndingSceneManager>(FindObjectsInactive.Include);
        if (ending != null)
        {
            ending.Play();
            return;
        }
        Debug.LogWarning("[DayEndHandler] Brak EndingSceneManager w scenie - pokazuję ekran statystyk.");

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
            pop: radioStation.currentListeners.pop,
            popDiff: popDiff
        );
    }

    public void Initialize(RadioStation rs, TimeHandler th, GameManager gm)
    {
        radioStation = rs;
        timeHandler = th;
        gameManager = gm;

        timeHandler.OnDayStarted += HandleDayStart;
        timeHandler.OnGameFinished += () => { gameFinished = true; };

        startListeners = new GenreValues
        {
            hipHop = radioStation.currentListeners.hipHop,
            disco = radioStation.currentListeners.disco,
            rock = radioStation.currentListeners.rock,
            pop = radioStation.currentListeners.pop
        };
        startMoney = radioStation.GetCurrentMoney();
        dailyOffer = new Cassette[3];
    }

    private Transform currentShopUI;

    public void GenerateDailyOffer(Transform shopUI)
    {
        currentShopUI = shopUI;
        if (allCassettes.Length < 3)
        {
            Debug.Log("Not enough cassettes to generate daily offer!");
            return;
        }

        // --- PATCH: MarketFlood — dynamiczna liczba slotów sklepu ---
        int shopSlots = 3;
        if (UpgradeManager.Instance != null)
            shopSlots += UpgradeManager.Instance.GetExtraShopSlots();
        shopSlots = Mathf.Min(shopSlots, allCassettes.Length);
        // --- KONIEC PATCHA ---

        int[] usedIndexes = new int[allCassettes.Length];
        for (int i = 0; i < usedIndexes.Length; i++)
            usedIndexes[i] = i;

        dailyOffer = new Cassette[shopSlots];

        for (int i = 0; i < shopSlots; i++)
        {
            int randomIndex = Random.Range(0, usedIndexes.Length - i);
            dailyOffer[i] = allCassettes[usedIndexes[randomIndex]];

            int temp = usedIndexes[randomIndex];
            usedIndexes[randomIndex] = usedIndexes[usedIndexes.Length - 1 - i];
            usedIndexes[usedIndexes.Length - 1 - i] = temp;
        }

        UpdateMoneySlotInShop();

        for (int i = 0; i < shopSlots && i < dailyOffer.Length; i++)
        {
            Transform slot = currentShopUI.Find($"Kaseta{i + 1}");
            if (slot == null) continue;

            TextMeshProUGUI name = slot.Find("NAZWA")
                                    .GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI price = slot.Find("CENA")
                                        .GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI stats = slot.Find("STATYSTYKI")
                                        .GetComponent<TextMeshProUGUI>();

            name.text = dailyOffer[i].name;
            dailyOffer[i].price = Random.Range(10, 100);
            price.text = dailyOffer[i].price.ToString() + " ZŁ";
            stats.text = dailyOffer[i].GetCassetteValues().ToString();
        }

        currentShopUI.gameObject.SetActive(true);
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


    public void BuyCassette(int offerIndex)
    {
        if (currentShopUI == null) return;
        Cassette cassetteToBuy = dailyOffer[offerIndex];
        float yourMoney = radioStation.GetCurrentMoney();
        float cassettePrice = cassetteToBuy.price;
        if (yourMoney >= cassettePrice)
        {
            radioStation.SetCurrentMoney(yourMoney - cassettePrice);
            UpdateMoneySlotInShop();
            Transform slot = currentShopUI.Find($"Kaseta{offerIndex + 1}");
            if (slot != null) slot.gameObject.SetActive(false);
            FMODUnity.RuntimeManager.PlayOneShot(buyingSound, this.transform.position);
        }

    }

    public void UpdateMoneySlotInShop()
    {
        if (currentShopUI == null) return;
        Transform yourMoneySlot = currentShopUI.Find("Money (wartość)");
        if (yourMoneySlot == null) yourMoneySlot = currentShopUI.Find("Money (wartosc)");

        if (yourMoneySlot != null)
        {
            TextMeshProUGUI yourMoney = yourMoneySlot.GetComponent<TextMeshProUGUI>();
            if (yourMoney != null)
            {
                yourMoney.text = radioStation.GetCurrentMoney().ToString() + " ZŁ";
            }
        }
    }

    public void ExitShop()
    {
        if (currentShopUI != null)
            currentShopUI.gameObject.SetActive(false);
        if (ShopButton != null)
            ShopButton.gameObject.SetActive(false);
        gameManager.SetInputEnabled(true);
    }
}