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

    public static DayEndHandler Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShopUI.gameObject.SetActive(false);
        startListeners = new GenreValues
        {
            hipHop = radioStation.currentListeners.hipHop,
            rock = radioStation.currentListeners.rock,
            pop = radioStation.currentListeners.pop,
            disco = radioStation.currentListeners.disco
        };
        startMoney = radioStation.GetCurrentMoney();

        ResetAllCassettePenalties();
    }

    void HandleDayStart()
    {   
        FMODUnity.RuntimeManager.PlayOneShot(yawnSound, this.transform.position);

        if (radioStation.currentListeners.hipHop <= 0 || radioStation.currentListeners.disco <= 0 || radioStation.currentListeners.pop <= 0 || radioStation.currentListeners.rock <= 0)
        {
            HandleGameFinished();
        }

        if (timeHandler.getDay() > 1)
        {
            int index = timeHandler.getDay() - 2;
            kawalerka_fee = index < kawalerka_fees.Length ? kawalerka_fees[index] : 0f;
            jedzenie_fee = index < jedzenie_fees.Length ? jedzenie_fees[index] : 0f;
            studia_fee = index < studia_fees.Length ? studia_fees[index] : 0f;
        }
        
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
        dailyOffer = new GameObject[3];
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

        // --- PATCH: MarketFlood — dynamiczna liczba slotów sklepu ---
        int shopSlots = 3;
        if (UpgradeManager.Instance != null)
            shopSlots += UpgradeManager.Instance.GetExtraShopSlots();
        
        int maxPossibleSlots = shopSlots;
        shopSlots = Mathf.Min(shopSlots, availableCassettes.Count);
        // --- KONIEC PATCHA ---

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

                // Statystyki kasety w procentach: minus na czerwono, zero/plus na zielono.
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

                // --- Spójny wygląd + czytelny układ ---
                // Czcionkę i materiał kopiujemy z tytułu (NAZWA), żeby wszystkie napisy wyglądały
                // jednakowo niezależnie od overrideów prefaba. Kolory gatunku i statystyk zostają własne.
                if (name != null)
                {
                    SetAnchoredY(name, -76f); // tytuł niżej, żeby grafika kasety na niego nie nachodziła

                    if (stats != null)
                    {
                        stats.font = name.font;
                        stats.fontSharedMaterial = name.fontSharedMaterial;
                        stats.color = name.color; // nazwy gatunków w kolorze reszty (pomarańczowym)
                        stats.fontSize = 19f; // większe statystyki
                        SetAnchoredY(stats, 120f); // wyżej, by większy 4-liniowy blok zmieścił się nad grafiką
                    }
                    if (genre != null)
                    {
                        genre.font = name.font;
                        genre.fontSharedMaterial = name.fontSharedMaterial;
                        genre.fontSize = 26f; // większy gatunek
                        genre.alignment = TMPro.TextAlignmentOptions.Center; // wyśrodkowany nad slotem
                        if (stats != null) genre.rectTransform.anchoredPosition = new Vector2(0f, GetAnchoredY(stats) + 42f);
                    }
                    if (author != null)
                    {
                        author.font = name.font;
                        author.fontSharedMaterial = name.fontSharedMaterial;
                        author.color = name.color; // ten sam kolor co tytuł
                        author.fontStyle = TMPro.FontStyles.Italic; // autor kursywą, jak tytuł
                        author.fontSize = 16f;
                        SetAnchoredY(author, GetAnchoredY(name) - 30f); // pod tytułem
                    }
                    if (price != null) SetAnchoredY(price, -130f); // cena na dole, z odstępem od autora
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

    /// <summary>
    /// Pojedyncza linia statystyk gatunku w %: ujemna na czerwono, zero/dodatnia na zielono.
    /// </summary>
    private static string StatLine(string label, int value)
    {
        string color = value < 0 ? "#E74C3C" : "#2ECC71"; // czerwony / zielony
        // Nazwa gatunku zostaje w bazowym kolorze napisu; kolorowa tylko wartość "liczba %".
        return $"{label}: <color={color}>{value}%</color>";
    }

    /// <summary>
    /// Kolor etykiety gatunku odpowiadający danemu gatunkowi muzycznemu.
    /// </summary>
    private static Color GetGenreColor(string genre)
    {
        switch ((genre ?? "").Trim().ToLowerInvariant())
        {
            case "pop":               return new Color32(255, 79, 163, 255); // róż
            case "rock":              return new Color32(231, 76, 60, 255);  // czerwony
            case "hip-hop":
            case "hiphop":            return new Color32(225, 161, 0, 255);  // złoty
            case "disco":             return new Color32(155, 89, 182, 255); // fiolet
            default:                  return Color.white;
        }
    }

    /// <summary>
    /// Resetuje dzienne "kary" kaset (timesUsed oraz zdegradowane lastCassetteValues),
    /// żeby następnego dnia modyfikatory wróciły do wartości bazowych (cassetteValues).
    /// Szuka wszystkich PlayableObject w scenie (także nieaktywnych, np. kupionych kaset
    /// czekających w sklepie), więc nie zależy od ręcznie przypisanej tablicy allCassettes
    /// ani od dynamicznie tworzonych kopii.
    /// </summary>
    private void ResetAllCassettePenalties()
    {
        var playables = FindObjectsByType<PlayableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in playables)
        {
            if (p == null || p.data == null) continue;
            p.data.ResetTimesUsed();
            p.data.ResetLastValues();
        }

        // Bazowe assety reklam (klony są niszczone osobno na koniec dnia).
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