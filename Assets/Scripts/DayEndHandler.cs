using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DayEndHandler : MonoBehaviour
{
    public int MinListenersToWin;
    public GameManager gameManager;
    public TimeHandler timeHandler;
    public Canvas ShopUI;
    private GenreValues startListeners;
    private float startMoney;
    public GameObject[] allCassettes;
    public Ad[] allAds;
    public GameObject[] dailyOffer;
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
    public Canvas dayIndicatorCanvas;
    public static DayEndHandler Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        ShopUI.gameObject.SetActive(false);
        
        if (radioStation != null)
        {
            CacheInitialState();
        }

        ResetAllCassettePenalties();
    }

    private void CacheInitialState()
    {
        if (radioStation == null) return;

        startListeners = new GenreValues
        {
            hipHop = radioStation.currentListeners.hipHop,
            rock = radioStation.currentListeners.rock,
            pop = radioStation.currentListeners.pop,
            disco = radioStation.currentListeners.disco
        };
        startMoney = radioStation.GetCurrentMoney();
    }

    public void Initialize(RadioStation rs, TimeHandler th, GameManager gm)
    {
        radioStation = rs;
        timeHandler = th;
        gameManager = gm;

        timeHandler.OnFirstDayStarted += HandleFirstDayStart;
        timeHandler.OnDayStarted += HandleDayStart;
        timeHandler.OnGameFinished += () => { gameFinished = true; };

        CacheInitialState();
        dailyOffer = new GameObject[3];

        if (timeHandler != null && timeHandler.getDay() == 1)
        {
            HandleFirstDayStart();
        }
    }

    public void HandleFirstDayStart()
    {
        if (timeHandler != null)
        {
            timeHandler.OnFirstDayStarted -= HandleFirstDayStart;
        }
        


        gameManager.SetInputEnabled(false);
        ResetAllCassettePenalties();

        radioStation.SetDailyListenersModifier(0f, 0f, 0f, 0f);
        radioStation.SetDailyRevenueModifier(0f, 0f, 0f, 0f);

        DaySummaryData.Day = 1; 
        DaySummaryData.RentFee = 0f;
        DaySummaryData.FoodFee = 0f;
        DaySummaryData.StudiesFee = 0f;
        DaySummaryData.FinalMoney = radioStation.GetCurrentMoney();
        DaySummaryData.MoneyDiff = 0f;
        DaySummaryData.AdsPenalty = 0f;
        DaySummaryData.UnplayedPenalties = new List<AdContractManager.UnplayedAdPenalty>();

        DaySummaryData.HipHop = radioStation.currentListeners.hipHop;
        DaySummaryData.HipHopDiff = 0;
        DaySummaryData.Disco = radioStation.currentListeners.disco;
        DaySummaryData.DiscoDiff = 0;
        DaySummaryData.Rock = radioStation.currentListeners.rock;
        DaySummaryData.RockDiff = 0;
        DaySummaryData.Pop = radioStation.currentListeners.pop;
        DaySummaryData.PopDiff = 0;

        Camera mainCam = gameManager.mainCamera;

        DaySummaryData.OnSummaryClosed = () =>
        {
            startListeners.hipHop = radioStation.currentListeners.hipHop;
            startListeners.disco = radioStation.currentListeners.disco;
            startListeners.rock = radioStation.currentListeners.rock;
            startListeners.pop = radioStation.currentListeners.pop;
            startMoney = radioStation.GetCurrentMoney();

            if (mainCam != null)
                mainCam.gameObject.SetActive(true);
            
            gameManager.SetInputEnabled(true);
        };

        DaySummaryData.OnSummaryClosed?.Invoke();
        DaySummaryData.OnSummaryClosed = null;
        dayIndicatorCanvas.GetComponent<CanvasFadeOut>()
        .startShowingDay(timeHandler.getDay());
    }

    private void OnDaySummarySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "DaySummaryScene")
        {
            SceneManager.sceneLoaded -= OnDaySummarySceneLoaded;

            GameObject shopInSummary = GameObject.Find("ShopUI"); 
            if (shopInSummary == null) shopInSummary = GameObject.Find("Ulepszenia");

            if (shopInSummary != null)
            {
                shopInSummary.SetActive(false);
            }
        }
    }

    void HandleDayStart()
    {   
        

        if (timeHandler != null && timeHandler.getDay() <= 1)
        {
            Debug.Log("[DayEndHandler] Zablokowano HandleDayStart dla dnia 1, aby uniknąć ulepszeń na starcie.");
            return; 
        }

        FMODUnity.RuntimeManager.PlayOneShot(yawnSound, this.transform.position);

        if (radioStation.currentListeners.hipHop <= 0 || radioStation.currentListeners.disco <= 0 || radioStation.currentListeners.pop <= 0 || radioStation.currentListeners.rock <= 0)
        {
            HandleGameFinished();
            return; 
        }

        int index = timeHandler.getDay() - 2;
        kawalerka_fee = index < kawalerka_fees.Length && index >= 0 ? kawalerka_fees[index] : 0f;
        jedzenie_fee = index < jedzenie_fees.Length && index >= 0 ? jedzenie_fees[index] : 0f;
        studia_fee = index < studia_fees.Length && index >= 0 ? studia_fees[index] : 0f;
        
        gameManager.SetInputEnabled(false);
        radioStation.SetCurrentMoney(radioStation.GetCurrentMoney() - kawalerka_fee - jedzenie_fee - studia_fee);

        ResetAllCassettePenalties();

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
            dayIndicatorCanvas.GetComponent<CanvasFadeOut>()
            .startShowingDay(timeHandler.getDay());
            gameManager.SetInputEnabled(true);

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
        else if (radioStation.GetTotalListeners() < MinListenersToWin)
            endGameCause = 2;

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

    private Transform currentShopUI;

    public void GenerateDailyOffer(Transform shopUI)
    {
        currentShopUI = shopUI;

        List<GameObject> availableCassettes = new List<GameObject>();
        foreach (var cassette in allCassettes)
        {
            if (!cassette.activeSelf) 
            {
                availableCassettes.Add(cassette);
            }
        }

        int shopSlots = 3;
        if (UpgradeManager.Instance != null)
            shopSlots += UpgradeManager.Instance.GetExtraShopSlots();
        
        int maxPossibleSlots = shopSlots;
        shopSlots = Mathf.Min(shopSlots, availableCassettes.Count);

        dailyOffer = new GameObject[shopSlots];

        for (int i = 0; i < shopSlots; i++)
        {
            int randomIndex = Random.Range(0, availableCassettes.Count);
            dailyOffer[i] = availableCassettes[randomIndex];
            availableCassettes.RemoveAt(randomIndex);
        }

        UpdateMoneySlotInShop();

        for (int i = 0; i < maxPossibleSlots; i++)
        {
            Transform slot = currentShopUI.Find($"Kaseta{i + 1}");
            if (slot == null) continue;

            if (i < shopSlots)
            {
                slot.gameObject.SetActive(true);
                PlayableContent data = dailyOffer[i].GetComponent<PlayableObject>().data;

                TextMeshProUGUI name = slot.Find("NAZWA")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI price = slot.Find("CENA")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI author = slot.Find("AUTOR")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI genre = slot.Find("GATUNEK")?.GetComponent<TextMeshProUGUI>();

                data.price = Random.Range(10, 100);

                if (name != null) name.text = $"\"{data.GetName()}\"";
                if (author != null) author.text = data.GetAuthor();
                if (genre != null)
                {
                    genre.text = data.GetGenre();
                    genre.color = GetGenreColor(data.GetGenre());
                }
                if (price != null) price.text = data.price.ToString() + " ZŁ";

                TextMeshProUGUI stats = slot.Find("STATYSTYKI")?.GetComponent<TextMeshProUGUI>();
                if (stats != null)
                {
                    stats.gameObject.SetActive(true);
                    GenreValues v = data.GetCassetteValues();
                    stats.text =
                        StatLine("Hip-Hop", v.hipHop) + "\n" +
                        StatLine("Disco", v.disco) + "\n" +
                        StatLine("Rock", v.rock) + "\n" +
                        StatLine("Pop", v.pop);
                }

                if (name != null)
                {
                    SetAnchoredY(name, -76f);

                    if (stats != null)
                    {
                        stats.font = name.font;
                        stats.fontSharedMaterial = name.fontSharedMaterial;
                        stats.color = name.color;
                        stats.fontSize = 19f;
                        SetAnchoredY(stats, 120f);
                    }
                    if (genre != null)
                    {
                        genre.font = name.font;
                        genre.fontSharedMaterial = name.fontSharedMaterial;
                        genre.fontSize = 26f;
                        genre.alignment = TMPro.TextAlignmentOptions.Center;
                        if (stats != null) genre.rectTransform.anchoredPosition = new Vector2(0f, GetAnchoredY(stats) + 42f);
                    }
                    if (author != null)
                    {
                        author.font = name.font;
                        author.fontSharedMaterial = name.fontSharedMaterial;
                        author.color = name.color;
                        author.fontStyle = TMPro.FontStyles.Italic;
                        author.fontSize = 16f;
                        SetAnchoredY(author, GetAnchoredY(name) - 30f);
                    }
                    if (price != null) SetAnchoredY(price, -130f);
                }
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }

        currentShopUI.gameObject.SetActive(true);
    }

    public void AddCassetteToOffer(GameObject cassette)
    {
        var newOffer = new GameObject[dailyOffer.Length + 1];
        newOffer[0] = cassette;
        for (int i = 0; i < dailyOffer.Length; i++)
            newOffer[i + 1] = dailyOffer[i];
        dailyOffer = newOffer;
        Debug.Log($"[DayEndHandler] Dodano '{cassette.name}' do oferty.");
    }

    public void BuyCassette(int offerIndex)
    {
        if (currentShopUI == null) return;
        GameObject cassetteToBuy = dailyOffer[offerIndex];
        float yourMoney = radioStation.GetCurrentMoney();
        float cassettePrice = cassetteToBuy.GetComponent<PlayableObject>().data.price;
        if (yourMoney >= cassettePrice)
        {
            radioStation.SetCurrentMoney(yourMoney - cassettePrice);
            UpdateMoneySlotInShop();
            Transform slot = currentShopUI.Find($"Kaseta{offerIndex + 1}");
            if (slot != null) slot.gameObject.SetActive(false);
            FMODUnity.RuntimeManager.PlayOneShot(buyingSound, this.transform.position);
            cassetteToBuy?.SetActive(true);
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

        gameManager.SetInputEnabled(true);
    }

    private static float GetAnchoredY(TMPro.TextMeshProUGUI t)
    {
        return t.rectTransform.anchoredPosition.y;
    }

    private static void SetAnchoredY(TMPro.TextMeshProUGUI t, float y)
    {
        var rt = t.rectTransform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
    }

    private static string StatLine(string label, int value)
    {
        string color = value < 0 ? "#E74C3C" : "#2ECC71";
        return $"{label}: <color={color}>{value}%</color>";
    }

    private static Color GetGenreColor(string genre)
    {
        switch ((genre ?? "").Trim().ToLowerInvariant())
        {
            case "pop":               return new Color32(255, 79, 163, 255);
            case "rock":              return new Color32(231, 76, 60, 255);
            case "hip-hop":
            case "hiphop":            return new Color32(225, 161, 0, 255);
            case "disco":             return new Color32(155, 89, 182, 255);
            default:                  return Color.white;
        }
    }

    private void ResetAllCassettePenalties()
    {
        var playables = FindObjectsByType<PlayableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in playables)
        {
            if (p == null || p.data == null) continue;
            p.data.ResetTimesUsed();
            p.data.ResetLastValues();
        }

        if (allAds != null)
        {
            foreach (var ad in allAds)
            {
                if (ad == null) continue;
                ad.ResetTimesUsed();
                ad.ResetLastValues();
            }
        }
    }
}