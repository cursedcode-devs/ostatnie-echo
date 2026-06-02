using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Zarządza wszystkimi zakupionymi ulepszeniami gracza oraz
/// wyświetla proceduralny ekran draftu po sklepie.
///
/// SETUP:
///   1. Dodaj ten komponent do GameManager GameObject
///   2. Przypisz allUpgrades[] — wszystkie SO ulepszeń
///   3. DaySummarySceneManager.OnShopContinueClicked() wywołuje
///      UpgradeManager.Instance.ShowDraftScreen(onFinished)
///
/// WYWOŁANIE:
///   UpgradeManager.Instance.ShowDraftScreen(() => { /* następny dzień */ });
///   bool has = UpgradeManager.Instance.HasUpgrade(UpgradeType.Back2Back);
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // ------------------------------------------------------------------
    [Header("Pula wszystkich ulepszeń")]
    public UpgradeDefinition[] allUpgrades;

    [Header("Ile opcji w drafcie")]
    public int draftChoices = 3;

    // ------------------------------------------------------------------
    // Stan — zakupione ulepszenia (mogą się powtarzać jeśli gracz kupi 2x)
    private List<UpgradeType> purchasedUpgrades = new List<UpgradeType>();
    // Ile razy każde ulepszenie zostało kupione (dla stackowalnych)
    private Dictionary<UpgradeType, int> upgradeStacks = new Dictionary<UpgradeType, int>();

    // Back2Back — czy już użyto w tej godzinie
    private bool back2BackUsedThisHour = false;
    // LuckyDraw — czy dostępny reroll w sklepie
    private bool luckyDrawAvailable = false;

    // ------------------------------------------------------------------
    private GameObject uiCanvas;
    private Action onDraftFinished;
    private List<UpgradeDefinition> currentDraftOptions = new List<UpgradeDefinition>();

    // ------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ------------------------------------------------------------------
    #region Public API

    /// <summary>Czy gracz posiada dane ulepszenie (co najmniej 1 stos).</summary>
    public bool HasUpgrade(UpgradeType type) => upgradeStacks.ContainsKey(type) && upgradeStacks[type] > 0;

    /// <summary>Ile razy gracz kupił dane ulepszenie.</summary>
    public int GetStacks(UpgradeType type) => upgradeStacks.ContainsKey(type) ? upgradeStacks[type] : 0;

    // --- Back2Back ---
    /// <summary>Wywołaj z RadioStation.CalculateSegment przed obliczeniem slotu.</summary>
    public bool TryConsumeBack2Back()
    {
        if (!HasUpgrade(UpgradeType.Back2Back)) return false;
        if (back2BackUsedThisHour) return false;
        back2BackUsedThisHour = true;
        return true;
    }

    /// <summary>Resetuj Back2Back — wywołaj na początku każdej godziny (NextHour).</summary>
    public void ResetBack2Back() => back2BackUsedThisHour = false;

    // --- LuckyDraw ---
    public bool HasLuckyDraw() => luckyDrawAvailable;
    public void ConsumeLuckyDraw() => luckyDrawAvailable = false;

    // --- NewHorizons bonus (mnożnik per gatunek, +5 per stos) ---
    public float GetNewHorizonsBonus()
    {
        if (!HasUpgrade(UpgradeType.NewHorizons)) return 0f;
        int stacks = GetStacks(UpgradeType.NewHorizons);
        // Znajdź definicję żeby pobrać wartość bonusu
        foreach (var def in allUpgrades)
            if (def.type == UpgradeType.NewHorizons)
                return def.newHorizonsBonus * stacks * 0.01f; // np. 5 → 0.05 modifier
        return stacks * 0.05f;
    }

    // --- Genre-specific bonuses (jako modifier 0-1+) ---
    public float GetGenreModifierBonus(UpgradeType genreType)
    {
        if (!HasUpgrade(genreType)) return 0f;
        int stacks = GetStacks(genreType);
        foreach (var def in allUpgrades)
            if (def.type == genreType)
                return def.genreBonus * stacks * 0.01f;
        return stacks * 0.10f;
    }

    // --- MarketFlood --- 
    public int GetExtraShopSlots()
    {
        if (!HasUpgrade(UpgradeType.MarketFlood)) return 0;
        int stacks = GetStacks(UpgradeType.MarketFlood);
        foreach (var def in allUpgrades)
            if (def.type == UpgradeType.MarketFlood)
                return def.marketFloodExtraSlots * stacks;
        return stacks * 2;
    }

    #endregion

    // ------------------------------------------------------------------
    #region Draft Screen

    /// <summary>
    /// Pokazuje ekran draftu ulepszeń. Wywołaj po zamknięciu sklepu.
    /// </summary>
    public void ShowDraftScreen(Action onFinished)
    {
        onDraftFinished = onFinished;

        // Losuj 3 opcje z puli
        currentDraftOptions = DrawDraftOptions();

        BuildDraftUI();
    }

    private List<UpgradeDefinition> DrawDraftOptions()
    {
        var result = new List<UpgradeDefinition>();
        if (allUpgrades == null || allUpgrades.Length == 0) return result;

        var pool = new List<UpgradeDefinition>(allUpgrades);
        int count = Mathf.Min(draftChoices, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int totalWeight = 0;
            foreach (var u in pool) totalWeight += u.weight;

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumul = 0;
            UpgradeDefinition picked = pool[0];

            foreach (var u in pool)
            {
                cumul += u.weight;
                if (roll < cumul) { picked = u; break; }
            }

            result.Add(picked);
            pool.Remove(picked);
        }

        return result;
    }

    private void BuildDraftUI()
    {
        if (uiCanvas != null) Destroy(uiCanvas);

        uiCanvas = new GameObject("UpgradeDraftCanvas");
        var canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 25;

        var scaler = uiCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        uiCanvas.AddComponent<GraphicRaycaster>();

        var ct = uiCanvas.transform;

        // Overlay
        var overlay = MakeImage(ct, "Overlay", new Color(0f, 0f, 0f, 0.82f));
        StretchFull(overlay);

        // Główny panel
        var panel = MakeImage(ct, "Panel", new Color32(14, 18, 24, 255));
        SR(panel, 0.5f, 0.5f, 1060, 580, 0, 0);
        var panelBorder = MakeImage(panel.transform, "Border", new Color32(30, 45, 65, 255));
        SR(panelBorder, 0.5f, 0.5f, 1040, 560, 0, 0);
        panelBorder.GetComponent<RectTransform>().SetAsFirstSibling();

        // Tytuł
        var titleTmp = MakeText(ct, "Title", "WYBIERZ ULEPSZENIE", 36,
            new Color32(220, 180, 50, 255)).GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle = FontStyles.Bold;
        SR(titleTmp.gameObject, 0.5f, 0.5f, 1000, 52, 0, 240);

        var subtitleGO = MakeText(ct, "Subtitle", "Możesz kupić jedno z poniższych ulepszeń lub pominąć.", 18,
            new Color32(120, 140, 170, 255));
        SR(subtitleGO, 0.5f, 0.5f, 1000, 34, 0, 198);

        SR(MakeImage(ct, "Div0", new Color32(40, 58, 80, 255)), 0.5f, 0.5f, 1000, 2, 0, 174);

        // Karty ulepszeń
        float[] cardX = { -340f, 0f, 340f };
        float cardY = 20f;

        RadioStation radio = null;
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null) radio = gm.radioStation;

        for (int i = 0; i < currentDraftOptions.Count; i++)
        {
            BuildUpgradeCard(ct, currentDraftOptions[i], cardX[i], cardY, radio, i);
        }

        // Przycisk Pomiń
        SR(MakeImage(ct, "DivBot", new Color32(40, 58, 80, 255)), 0.5f, 0.5f, 1000, 2, 0, -168);
        var skipBtn = MakeButton(ct, "SkipBtn", "POMIŃ →", new Color32(28, 35, 46, 255), new Color32(160, 140, 80, 255));
        SR(skipBtn, 0.5f, 0.5f, 240, 54, 0, -220);
        skipBtn.GetComponent<Button>().onClick.AddListener(OnSkipClicked);

        Time.timeScale = 0f;
    }

    private void BuildUpgradeCard(Transform parent, UpgradeDefinition upgrade, float x, float y, RadioStation radio, int index)
    {
        bool canAfford = radio != null && radio.GetCurrentMoney() >= upgrade.cost;

        // Karta tło
        Color32 cardBg = canAfford
            ? new Color32(20, 28, 38, 255)
            : new Color32(16, 18, 22, 255);

        var card = MakeImage(parent, $"Card_{index}", cardBg);
        SR(card, 0.5f, 0.5f, 300, 300, x, y);

        var cardBorder = MakeImage(card.transform, "Border",
            canAfford ? new Color32(38, 58, 86, 255) : new Color32(28, 30, 36, 255));
        SR(cardBorder, 0.5f, 0.5f, 288, 288, 0, 0);
        cardBorder.GetComponent<RectTransform>().SetAsFirstSibling();

        // Typ badge (górny pasek)
        bool isHard = upgrade.type == UpgradeType.Back2Back ||
                      upgrade.type == UpgradeType.RollBack ||
                      upgrade.type == UpgradeType.LuckyDraw;

        var badge = MakeImage(card.transform, "Badge",
            isHard ? new Color32(60, 30, 10, 255) : new Color32(15, 45, 25, 255));
        SR(badge, 0.5f, 0.5f, 288, 28, 0, 130);

        var badgeTmp = MakeText(badge.transform, "BadgeLabel",
            isHard ? "ZAAWANSOWANE" : "PODSTAWOWE", 12,
            isHard ? new Color32(220, 140, 60, 255) : new Color32(80, 200, 100, 255))
            .GetComponent<TextMeshProUGUI>();
        badgeTmp.fontStyle = FontStyles.Bold;
        StretchFull(badgeTmp.gameObject);

        // Nazwa
        var nameTmp = MakeText(card.transform, "Name", upgrade.upgradeName, 20,
            canAfford ? new Color32(230, 210, 255, 255) : new Color32(100, 100, 110, 255))
            .GetComponent<TextMeshProUGUI>();
        nameTmp.fontStyle = FontStyles.Bold;
        SR(nameTmp.gameObject, 0.5f, 0.5f, 270, 52, 0, 80);

        // Opis
        var descTmp = MakeText(card.transform, "Desc", upgrade.description, 14,
            canAfford ? new Color32(160, 175, 200, 255) : new Color32(70, 75, 85, 255))
            .GetComponent<TextMeshProUGUI>();
        descTmp.alignment = TextAlignmentOptions.Center;
        SR(descTmp.gameObject, 0.5f, 0.5f, 268, 110, 0, -10);

        // Separator
        SR(MakeImage(card.transform, "Div", new Color32(40, 55, 75, 255)), 0.5f, 0.5f, 260, 1, 0, -72);

        // Cena
        Color32 priceColor = canAfford
            ? new Color32(80, 220, 100, 255)
            : new Color32(220, 80, 80, 255);

        var priceTmp = MakeText(card.transform, "Price", $"{upgrade.cost:F0}$", 22, priceColor)
            .GetComponent<TextMeshProUGUI>();
        priceTmp.fontStyle = FontStyles.Bold;
        SR(priceTmp.gameObject, 0.5f, 0.5f, 270, 36, 0, -100);

        // Przycisk KUP / BRAK ŚRODKÓW
        if (canAfford)
        {
            var buyBtn = MakeButton(card.transform, "BuyBtn", "KUP",
                new Color32(20, 55, 30, 255), new Color32(80, 220, 100, 255));
            SR(buyBtn, 0.5f, 0.5f, 220, 48, 0, -130);

            int idx = index; // capture
            buyBtn.GetComponent<Button>().onClick.AddListener(() => OnUpgradePurchased(idx));
        }
        else
        {
            var noBtn = MakeImage(card.transform, "NoMoneyBtn", new Color32(22, 22, 26, 255));
            SR(noBtn, 0.5f, 0.5f, 220, 48, 0, -130);
            var noTmp = MakeText(noBtn.transform, "NoLabel", "BRAK ŚRODKÓW", 14,
                new Color32(80, 80, 90, 255));
            StretchFull(noTmp);
        }
    }

    private void OnUpgradePurchased(int index)
    {
        if (index < 0 || index >= currentDraftOptions.Count) return;

        UpgradeDefinition upgrade = currentDraftOptions[index];
        var gm = FindFirstObjectByType<GameManager>();
        if (gm == null || gm.radioStation == null) return;

        RadioStation radio = gm.radioStation;

        // Odejmij kasę
        radio.SetCurrentMoney(radio.GetCurrentMoney() - upgrade.cost);

        // Zastosuj ulepszenie
        ApplyUpgrade(upgrade, radio, gm);

        Debug.Log($"[UpgradeManager] Zakupiono: {upgrade.upgradeName} za {upgrade.cost}$");

        CloseDraftUI();
    }

    private void OnSkipClicked()
    {
        Debug.Log("[UpgradeManager] Pominięto wybór ulepszenia.");
        CloseDraftUI();
    }

    private void CloseDraftUI()
    {
        Time.timeScale = 1f;
        if (uiCanvas != null) Destroy(uiCanvas);
        onDraftFinished?.Invoke();
    }

    #endregion

    // ------------------------------------------------------------------
    #region Apply Logic

    private void ApplyUpgrade(UpgradeDefinition upgrade, RadioStation radio, GameManager gm)
    {
        // Zarejestruj w liście
        purchasedUpgrades.Add(upgrade.type);
        if (!upgradeStacks.ContainsKey(upgrade.type))
            upgradeStacks[upgrade.type] = 0;
        upgradeStacks[upgrade.type]++;

        switch (upgrade.type)
        {
            // -- Back2Back: logika w RadioStation.CalculateSegment (sprawdza TryConsumeBack2Back)
            case UpgradeType.Back2Back:
                Debug.Log("[Upgrade] Back2Back aktywny — podwaja slot raz na godzinę.");
                break;

            // -- RollBack: czysci negatywne timesUsed z kaset
            case UpgradeType.RollBack:
                ApplyRollBack(gm);
                break;

            // -- LuckyDraw: odblokowuje reroll w sklepie
            case UpgradeType.LuckyDraw:
                luckyDrawAvailable = true;
                Debug.Log("[Upgrade] LuckyDraw — reroll sklepu odblokowany.");
                break;

            // -- MarketFlood: więcej slotów w sklepie (DayEndHandler.GenerateDailyOffer respektuje GetExtraShopSlots)
            case UpgradeType.MarketFlood:
                Debug.Log($"[Upgrade] MarketFlood — +{upgrade.marketFloodExtraSlots} slotów w sklepie.");
                break;

            // -- NewHorizons: +5% do wszystkich kategorii słuchaczy (permanentny dzienny modifier)
            case UpgradeType.NewHorizons:
            {
                float bonus = upgrade.newHorizonsBonus * 0.01f;
                radio.AddDailyListenersModifier(bonus, bonus, bonus, bonus);
                Debug.Log($"[Upgrade] NewHorizons — +{upgrade.newHorizonsBonus}% do wszystkich gatunków.");
                break;
            }

            // -- DiscoNight
            case UpgradeType.DiscoNight:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddDailyListenersModifier(0, bonus, 0, 0);
                Debug.Log($"[Upgrade] DiscoNight — +{upgrade.genreBonus}% Disco.");
                break;
            }

            // -- PopStars
            case UpgradeType.PopStars:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddDailyListenersModifier(0, 0, 0, bonus);
                Debug.Log($"[Upgrade] PopStars — +{upgrade.genreBonus}% Pop.");
                break;
            }

            // -- ComptonVibes
            case UpgradeType.ComptonVibes:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddDailyListenersModifier(bonus, 0, 0, 0);
                Debug.Log($"[Upgrade] ComptonVibes — +{upgrade.genreBonus}% HipHop.");
                break;
            }

            // -- RockAndRoll
            case UpgradeType.RockAndRoll:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddDailyListenersModifier(0, 0, bonus, 0);
                Debug.Log($"[Upgrade] RockAndRoll — +{upgrade.genreBonus}% Rock.");
                break;
            }
        }
    }

    /// <summary>
    /// RollBack — kasuje negatywne ostatnie wartości kaset (przywraca oryginalne).
    /// Szuka wszystkich PlayableObject w scenie i resetuje timesUsed.
    /// </summary>
    private void ApplyRollBack(GameManager gm)
    {
        var playables = FindObjectsByType<PlayableObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var p in playables)
        {
            if (p.data != null && p.data.GetType() == CassetteTypes.Music && p.data.GetTimesUsed() > 0)
            {
                p.data.ResetTimesUsed();
                p.data.ResetLastValues();
                count++;
            }
        }
        Debug.Log($"[Upgrade] RollBack — zresetowano {count} kaset.");
    }

    #endregion

    // ------------------------------------------------------------------
    #region UI Helpers

    GameObject MakeImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<Image>().color = color;
        return obj;
    }

    GameObject MakeText(Transform parent, string name, string text, int size, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return obj;
    }

    GameObject MakeButton(Transform parent, string name, string label, Color32 bgColor, Color32 textColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color32(
            (byte)Mathf.Min(bgColor.r + 20, 255),
            (byte)Mathf.Min(bgColor.g + 20, 255),
            (byte)Mathf.Min(bgColor.b + 20, 255), 255);
        cb.pressedColor = new Color32(220, 180, 50, 255);
        btn.colors = cb;
        var lbl = MakeText(obj.transform, "Label", label, 18, textColor);
        lbl.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        StretchFull(lbl);
        return obj;
    }

    void SR(GameObject obj, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = obj.GetComponent<RectTransform>();
        if (!rt) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    void StretchFull(GameObject obj)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    #endregion
}
