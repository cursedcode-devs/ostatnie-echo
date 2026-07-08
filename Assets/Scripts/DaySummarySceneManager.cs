using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Menedżer sceny podsumowującej dzień.
/// Odczytuje statystyki z DaySummaryData i przypisuje je do podpiętych elementów UI.
/// </summary>
public class DaySummarySceneManager : MonoBehaviour
{
    [Header("Główne")]
    public List<TextMeshProUGUI> titleTexts;

    [Header("Koszty")]
    public TextMeshProUGUI rentFeeText;
    public TextMeshProUGUI foodFeeText;
    public TextMeshProUGUI studiesFeeText;

    [Header("Kary z reklam")]
    public TextMeshProUGUI adsPenaltyText;
    public TextMeshProUGUI adsPenaltyBreakdownText;

    [Header("Pieniądze")]
    public TextMeshProUGUI moneyFinalText;
    public TextMeshProUGUI moneyDiffText;

    [Header("Gatunki (Teraz)")]
    public TextMeshProUGUI hipHopFinalText;
    public TextMeshProUGUI discoFinalText;
    public TextMeshProUGUI rockFinalText;
    public TextMeshProUGUI popFinalText;

    [Header("Gatunki (Zmiana)")]
    public TextMeshProUGUI hipHopDiffText;
    public TextMeshProUGUI discoDiffText;
    public TextMeshProUGUI rockDiffText;
    public TextMeshProUGUI popDiffText;

    [Header("Ekrany (Panele)")]
    public GameObject budgetPanel;
    public GameObject listenersPanel;
    public GameObject shopPanel;
    public GameObject newspaperPanel;
    public GameObject contractsPanel;

    [Header("Przyciski Kontynuacji")]
    public Button budgetContinueButton;
    public Button listenersContinueButton;
    public Button shopContinueButton;
    public Button newspaperContinueButton;
    public Button contractsContinueButton;

    [Header("Kontrakty")]
    public Transform contractsContainer;
    private List<Toggle> adToggles = new List<Toggle>();
    private List<Ad> currentDailyOffers = new List<Ad>();

    // Sklep ulepszeń (panel budowany w czasie działania, w stylu ekranu kontraktów)
    private UpgradeManager upgradeManager;
    private List<UpgradeDefinition> upgradeOptions;
    private GameObject upgradePanel;
    private TextMeshProUGUI upgradeMoneyText;

    void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("DebugEventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("[DEBUG] Utworzono EventSystem (dla testów), ponieważ żaden nie istniał w scenie.");
        }
#endif

        Time.timeScale = 0f;

        BindDataToUI();

        // --- FIXED DAY 1 MORNING STATE SEQUENCE ---
        if (DaySummaryData.Day == 1)
        {
            // Przywróć czas i wywołaj akcję zamykającą podsumowanie
            Time.timeScale = 1f;
            DaySummaryData.OnSummaryClosed?.Invoke();
            DaySummaryData.OnSummaryClosed = null;
            // Wyładuj scenę podsumowania bez pokazywania telegazety ani kontraktów
            SceneManager.UnloadSceneAsync("DaySummaryScene");
            return; // Zakończ działanie funkcji Start w tym miejscu
        }
        else
        {
            if (budgetPanel != null) budgetPanel.SetActive(true);
            if (listenersPanel != null) listenersPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
        }

        // Setup clear button interactions
        if (budgetContinueButton != null)
        {
            budgetContinueButton.onClick.RemoveAllListeners();
            budgetContinueButton.onClick.AddListener(OnBudgetContinueClicked);
        }

        if (listenersContinueButton != null)
        {
            listenersContinueButton.onClick.RemoveAllListeners();
            listenersContinueButton.onClick.AddListener(OnListenersContinueClicked);
        }

        if (shopContinueButton != null)
        {
            shopContinueButton.onClick.RemoveAllListeners();
            shopContinueButton.onClick.AddListener(OnShopContinueClicked);
        }

        if (contractsContinueButton != null)
        {
            contractsContinueButton.onClick.RemoveAllListeners();
            contractsContinueButton.onClick.AddListener(OnContractsContinueClicked);
        }

        if (newspaperContinueButton != null)
        {
            newspaperContinueButton.onClick.RemoveAllListeners();
            newspaperContinueButton.onClick.AddListener(OnNewspaperMorningContinueClicked);
        }
    }

    private void OnNewspaperMorningContinueClicked()
    {
        if (newspaperPanel != null) newspaperPanel.SetActive(false);

        if (DaySummaryData.Day == 1 && contractsPanel != null)
        {
            contractsPanel.SetActive(true);
            BuildContractsUI();
        }
        else
        {
            ShowUpgradeOrNewspaper();
        }
    }

    void BindDataToUI()
    {
        if (titleTexts != null)
        {
            foreach (var t in titleTexts)
            {
                if (t != null)
                {
                    // FIX: Stop title from reading 'KONIEC DNIA 0' on fresh game load
                    t.text = (DaySummaryData.Day == 1) ? "PORANEK DNIA 1" : $"KONIEC DNIA {DaySummaryData.Day - 1}";
                }
            }
        }

        if (rentFeeText != null) rentFeeText.text = $"{DaySummaryData.RentFee:F2}ZŁ";
        if (foodFeeText != null) foodFeeText.text = $"{DaySummaryData.FoodFee:F2}ZŁ";
        if (studiesFeeText != null) studiesFeeText.text = $"{DaySummaryData.StudiesFee:F2}ZŁ";

        if (adsPenaltyText != null) adsPenaltyText.text = $"{DaySummaryData.AdsPenalty:F2}ZŁ";
        if (adsPenaltyBreakdownText != null)
        {
            if (DaySummaryData.UnplayedPenalties != null && DaySummaryData.UnplayedPenalties.Count > 0)
            {
                string bText = "Niewyemitowane zlecenia (kara 1/2 zysku):\n";
                foreach (var p in DaySummaryData.UnplayedPenalties)
                {
                    bText += $" • {p.clientName} (\"{p.adTitle}\"): -{p.penaltyAmount:F2}ZŁ\n";
                }
                adsPenaltyBreakdownText.text = bText;
                adsPenaltyBreakdownText.gameObject.SetActive(true);
            }
            else
            {
                adsPenaltyBreakdownText.text = "";
                adsPenaltyBreakdownText.gameObject.SetActive(false);
            }
        }

        if (moneyFinalText != null) moneyFinalText.text = $"{DaySummaryData.FinalMoney:F2}ZŁ";
        if (moneyDiffText != null)
        {
            moneyDiffText.text = FormatDiff(DaySummaryData.MoneyDiff, "F2", "ZŁ");
        }

        if (hipHopFinalText != null) hipHopFinalText.text = $"{DaySummaryData.HipHop}";
        if (discoFinalText != null) discoFinalText.text = $"{DaySummaryData.Disco}";
        if (rockFinalText != null) rockFinalText.text = $"{DaySummaryData.Rock}";
        if (popFinalText != null) popFinalText.text = $"{DaySummaryData.Pop}";

        if (hipHopDiffText != null)
        {
            hipHopDiffText.text = FormatDiff(DaySummaryData.HipHopDiff);
        }
        if (discoDiffText != null)
        {
            discoDiffText.text = FormatDiff(DaySummaryData.DiscoDiff);
        }
        if (rockDiffText != null)
        {
            rockDiffText.text = FormatDiff(DaySummaryData.RockDiff);
        }
        if (popDiffText != null)
        {
            popDiffText.text = FormatDiff(DaySummaryData.PopDiff);
        }
    }

    public void OnBudgetContinueClicked()
    {
        if (budgetPanel != null) budgetPanel.SetActive(false);
        if (listenersPanel != null) listenersPanel.SetActive(true);
    }

    public void OnListenersContinueClicked()
    {
        if (listenersPanel != null) listenersPanel.SetActive(false);

        //OSTATNI DZIEŃ ZMIEŃ TU!!!
        int lastDay = 3;

        if (DaySummaryData.Day > lastDay)
        {
            // Pomijamy sklep, kontrakty i ulepszenia.
            // Przechodzimy bezpośrednio do ładowania telegazety / zamykania podsumowania
            StartCoroutine(LoadNewspaperAndUnload());
            return;
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            if (DayEndHandler.Instance != null)
            {
                DayEndHandler.Instance.GenerateDailyOffer(shopPanel.transform);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("[DEBUG] Brak DayEndHandler. Generowanie placeholderów w sklepie.");
                for (int i = 0; i < 3; i++)
                {
                    Transform kaseta = shopPanel.transform.Find($"Kaseta{i + 1}");
                    if (kaseta != null)
                    {
                        var title = kaseta.Find("NAZWA")?.GetComponent<TextMeshProUGUI>();
                        var price = kaseta.Find("CENA")?.GetComponent<TextMeshProUGUI>();
                        var stats = kaseta.Find("STATYSTYKI")?.GetComponent<TextMeshProUGUI>();

                        if (title != null) title.text = $"DEBUG KASETA {i + 1}";
                        if (price != null) price.text = $"{(i + 1) * 15} ZŁ";
                        if (stats != null) stats.text = "+10 Zadowolenia";
                    }
                }
#endif
            }
        }
    }

    public void OnShopContinueClicked()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (contractsPanel != null)
        {
            contractsPanel.SetActive(true);
            BuildContractsUI();
        }
        else
        {
            ShowUpgradeOrNewspaper();
        }
    }

    public void OnContractsContinueClicked()
    {
        if (contractsPanel != null)
            contractsPanel.SetActive(false);

        var adManager = FindFirstObjectByType<AdContractManager>();
        if (adManager != null)
        {
            List<Ad> selectedAds = new List<Ad>();

            for (int i = 0; i < adToggles.Count; i++)
            {
                if (adToggles[i].isOn)
                {
                    selectedAds.Add(currentDailyOffers[i]);
                }
            }

            adManager.AcceptContracts(selectedAds);
        }

        if (DaySummaryData.Day <= 1)
        {
            Debug.Log("[DaySummarySceneManager] Dzień 1: Pomijam ulepszenia. Ładuję gazetę.");
            StartCoroutine(LoadNewspaperAndUnload());
        }
        else
        {
            ShowUpgradeOrNewspaper();
        }
    }

    private void ShowUpgradeOrNewspaper()
    {
        if (DaySummaryData.Day <= 1)
        {
            StartCoroutine(LoadNewspaperAndUnload());
            return;
        }

        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        upgradeOptions = upgradeManager != null ? upgradeManager.GetOrCreateDraftOptions() : null;

        if (upgradeManager == null || upgradeOptions == null || upgradeOptions.Count == 0)
        {
            StartCoroutine(LoadNewspaperAndUnload());
            return;
        }

        BuildUpgradesUI();
    }

    // ------------------------------------------------------------------
    #region Sklep ulepszeń

    private static readonly Color32 ShopWhite = new Color32(245, 240, 232, 255);
    private static readonly Color32 ShopGreen = new Color32(70, 230, 95, 255);
    private static readonly Color32 ShopRed   = new Color32(220, 110, 95, 255);
    private static readonly Color32 ShopDim   = new Color32(205, 195, 182, 255);
    private static readonly Color32 ShopLine  = new Color32(235, 150, 60, 255);
    private const float SlotSpacing = 560f;

    private TMPro.TMP_FontAsset upgradeFont;
    private Material upgradeFontMat;

    private void BuildUpgradesUI()
    {
        CacheShopAssets();

        Transform canvas = GetSummaryCanvas();

        upgradePanel = new GameObject("UpgradePanel");
        upgradePanel.transform.SetParent(canvas, false);
        var rt = upgradePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        upgradePanel.transform.SetAsLastSibling();

        Transform root = upgradePanel.transform;

        var title = MakeShopText(root, "TYTUL", "ULEPSZENIA", 64, ShopWhite, TextAlignmentOptions.Center);
        SR(title.gameObject, 0.5f, 0.5f, 1000, 90, 0, 445);

        upgradeMoneyText = MakeShopText(root, "Kasa", "", 40, ShopWhite, TextAlignmentOptions.Right);
        SR(upgradeMoneyText.gameObject, 0.5f, 0.5f, 600, 56, 600, 445);

        SR(MakeImage(root, "LiniaGora", ShopLine), 0.5f, 0.5f, 1720, 4, 0, 385);
        SR(MakeImage(root, "LiniaDol", ShopLine), 0.5f, 0.5f, 1720, 4, 0, -345);

        var hint = MakeShopText(root, "Hint", "Każde ulepszenie możesz kupić tylko raz.", 26, ShopDim, TextAlignmentOptions.Center);
        SR(hint.gameObject, 0.5f, 0.5f, 1200, 36, 0, 335);

        var slotsRoot = new GameObject("Sloty");
        slotsRoot.transform.SetParent(root, false);
        SR(slotsRoot, 0.5f, 0.5f, 1800, 700, 0, -10);

        var continueBtn = MakeShopButton(root, "Dalej", "DALEJ", ShopWhite, new Color32(40, 25, 15, 255));
        SR(continueBtn, 0.5f, 0.5f, 340, 78, 0, -450);
        continueBtn.GetComponent<Button>().onClick.AddListener(OnUpgradesContinue);

        RebuildUpgradeCards(slotsRoot.transform);
    }

    private Transform GetSummaryCanvas()
    {
        if (contractsPanel != null) return contractsPanel.transform.parent;
        if (shopPanel != null) return shopPanel.transform.parent;
        if (budgetPanel != null) return budgetPanel.transform.parent;
        return transform;
    }

    private void CacheShopAssets()
    {
        TextMeshProUGUI src = null;
        if (shopPanel != null)
        {
            var tytul = shopPanel.transform.Find("TYTUŁ");
            if (tytul != null) src = tytul.GetComponent<TextMeshProUGUI>();
            if (src == null) src = shopPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        if (src == null && contractsPanel != null)
            src = contractsPanel.GetComponentInChildren<TextMeshProUGUI>(true);

        if (src != null)
        {
            upgradeFont = src.font;
            upgradeFontMat = src.fontSharedMaterial;
        }
    }

    private void RebuildUpgradeCards(Transform slotsRoot)
    {
        foreach (Transform c in slotsRoot) Destroy(c.gameObject);
        RefreshUpgradeMoney();

        int n = upgradeOptions.Count;
        float startX = -(n - 1) * SlotSpacing / 2f;
        for (int i = 0; i < n; i++)
            BuildUpgradeSlot(slotsRoot, upgradeOptions[i], startX + i * SlotSpacing);
    }

    private void BuildUpgradeSlot(Transform parent, UpgradeDefinition upgrade, float x)
    {
        float money = GetCurrentMoneySafe();
        bool owned = upgradeManager.HasUpgrade(upgrade.type);
        bool canAfford = !owned && money >= upgrade.cost;

        GetUpgradeInfo(upgrade, out string flavor, out string effectMain, out string effectSub);

        var slot = new GameObject("Slot");
        slot.transform.SetParent(parent, false);
        SR(slot, 0.5f, 0.5f, 480, 700, x, 0);

        var name = MakeShopText(slot.transform, "NAZWA", upgrade.upgradeName, 44, ShopWhite, TextAlignmentOptions.Center);
        SR(name.gameObject, 0.5f, 0.5f, 470, 56, 0, 250);

        var tag = MakeShopText(slot.transform, "TAG", flavor, 26, ShopDim, TextAlignmentOptions.Center);
        tag.fontStyle = FontStyles.Italic;
        SR(tag.gameObject, 0.5f, 0.5f, 440, 36, 0, 192);

        var eff = MakeShopText(slot.transform, "EFEKT", effectMain, 50, ShopGreen, TextAlignmentOptions.Center);
        SR(eff.gameObject, 0.5f, 0.5f, 460, 62, 0, 72);

        var effSub = MakeShopText(slot.transform, "EFEKT_SUB", effectSub, 30, ShopDim, TextAlignmentOptions.Center);
        SR(effSub.gameObject, 0.5f, 0.5f, 460, 40, 0, 14);

        var price = MakeShopText(slot.transform, "CENA", $"{upgrade.cost:F0} ZŁ", 46, ShopWhite, TextAlignmentOptions.Center);
        SR(price.gameObject, 0.5f, 0.5f, 440, 58, 0, -110);

        if (owned)
        {
            var b = MakeShopButton(slot.transform, "Kupiono", "KUPIONO", ShopGreen, new Color32(20, 40, 25, 255));
            b.GetComponent<Button>().interactable = false;
            SR(b, 0.5f, 0.5f, 320, 72, 0, -240);
        }
        else if (canAfford)
        {
            var buy = MakeShopButton(slot.transform, "KUP", "KUP", ShopWhite, new Color32(40, 25, 15, 255));
            SR(buy, 0.5f, 0.5f, 320, 72, 0, -240);
            UpgradeDefinition def = upgrade;
            Transform rootT = parent;
            buy.GetComponent<Button>().onClick.AddListener(() => OnUpgradeBuyClicked(def, rootT));
        }
        else
        {
            var b = MakeShopButton(slot.transform, "Brak", "BRAK ŚRODKÓW", ShopRed, ShopWhite);
            b.GetComponent<Button>().interactable = false;
            SR(b, 0.5f, 0.5f, 320, 72, 0, -240);
        }
    }

    private void GetUpgradeInfo(UpgradeDefinition u, out string flavor, out string effectMain, out string effectSub)
    {
        switch (u.type)
        {
            case UpgradeType.DiscoNight:
                flavor = "Lepszy target"; effectMain = $"+{u.genreBonus:F0}% mnożnika"; effectSub = "do kategorii Disco"; break;
            case UpgradeType.PopStars:
                flavor = "Lepszy target"; effectMain = $"+{u.genreBonus:F0}% mnożnika"; effectSub = "do kategorii Pop"; break;
            case UpgradeType.ComptonVibes:
                flavor = "Lepszy target"; effectMain = $"+{u.genreBonus:F0}% mnożnika"; effectSub = "do kategorii Hip-Hop"; break;
            case UpgradeType.RockAndRoll:
                flavor = "Lepszy target"; effectMain = $"+{u.genreBonus:F0}% mnożnika"; effectSub = "do kategorii Rock"; break;
            case UpgradeType.NewHorizons:
                flavor = "Lepsza antena"; effectMain = $"+{u.newHorizonsBonus:F0}% mnożnika"; effectSub = "do każdej kategorii"; break;
            default:
                flavor = ""; effectMain = u.description; effectSub = ""; break;
        }
    }

    private void OnUpgradeBuyClicked(UpgradeDefinition upgrade, Transform slotsRoot)
    {
        if (upgradeManager != null && upgradeManager.TryPurchase(upgrade))
            RebuildUpgradeCards(slotsRoot);
    }

    private void OnUpgradesContinue()
    {
        if (upgradeManager != null) upgradeManager.ClearDraftOptions();
        if (upgradePanel != null) Destroy(upgradePanel);
        StartCoroutine(LoadNewspaperAndUnload());
    }

    private void RefreshUpgradeMoney()
    {
        if (upgradeMoneyText != null)
            upgradeMoneyText.text = $"KASA: {GetCurrentMoneySafe():F0} ZŁ";
    }

    private float GetCurrentMoneySafe()
    {
        var gm = FindFirstObjectByType<GameManager>();
        return (gm != null && gm.radioStation != null) ? gm.radioStation.GetCurrentMoney() : 0f;
    }

    private TextMeshProUGUI MakeShopText(Transform parent, string name, string text, int size, Color color, TextAlignmentOptions align)
    {
        var go = MakeText(parent, name, text, size, color);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;
        if (upgradeFont != null) tmp.font = upgradeFont;
        if (upgradeFontMat != null) tmp.fontSharedMaterial = upgradeFontMat;
        return tmp;
    }

    private GameObject MakeShopButton(Transform parent, string name, string label, Color32 bg, Color32 textColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = bg;
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cb.selectedColor = Color.white;
        cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        cb.disabledColor = Color.white;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
        var lbl = MakeShopText(obj.transform, "Label", label, 30, textColor, TextAlignmentOptions.Center);
        StretchFull(lbl.gameObject);
        return obj;
    }

    #endregion

    private void BuildContractsUI()
    {
        if (contractsContainer == null) return;

        var adManager = FindFirstObjectByType<AdContractManager>();
        if (adManager == null) return;

        currentDailyOffers = adManager.GenerateDailyOffers(5);

        foreach (Transform child in contractsContainer)
        {
            Destroy(child.gameObject);
        }

        adToggles.Clear();

        float y = 0;
        float step = 115;

        for (int i = 0; i < currentDailyOffers.Count; i++)
        {
            Ad ad = currentDailyOffers[i];
            float payout = adManager.CalculatePotentialPayout(ad);

            var rowBox = MakeImage(contractsContainer, $"RowBox_{i}", new Color32(25, 31, 40, 255));
            SR(rowBox, 0.5f, 1f, 1020, 95, 0, -y - 50);
            var outline = rowBox.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color32(42, 58, 80, 255);

            var rowTransform = rowBox.transform;

            var clientTxt = MakeText(rowTransform, "Client", $"ZLECENIODAWCA: {ad.GetAuthor().ToUpper()}", 14, new Color32(220, 180, 50, 255));
            SR(clientTxt, 0.5f, 0.5f, 550, 24, -200, 20);
            var tmpClient = clientTxt.GetComponent<TextMeshProUGUI>();
            tmpClient.alignment = TextAlignmentOptions.Left;
            tmpClient.fontStyle = FontStyles.Bold;

            var titleTxt = MakeText(rowTransform, "Title", ad.GetName(), 20, new Color32(255, 255, 255, 255));
            SR(titleTxt, 0.5f, 0.5f, 550, 36, -200, -12);
            var tmpTitle = titleTxt.GetComponent<TextMeshProUGUI>();
            tmpTitle.alignment = TextAlignmentOptions.Left;
            tmpTitle.fontStyle = FontStyles.Bold;

            var payoutTag = MakeImage(rowTransform, "PayoutTag", new Color32(12, 45, 25, 255));
            SR(payoutTag, 0.5f, 0.5f, 220, 50, 220, 0);
            var payoutOutline = payoutTag.AddComponent<UnityEngine.UI.Outline>();
            payoutOutline.effectColor = new Color32(30, 90, 50, 255);

            var payoutTxt = MakeText(payoutTag.transform, "PayoutText", $"SZACOWANY\nZAROBEK: {payout:F2} ZŁ", 16, new Color32(80, 220, 100, 255));
            StretchFull(payoutTxt);
            var tmpPayout = payoutTxt.GetComponent<TextMeshProUGUI>();
            tmpPayout.alignment = TextAlignmentOptions.Center;
            tmpPayout.fontStyle = FontStyles.Bold;

            var toggleGO = new GameObject("Toggle");
            toggleGO.transform.SetParent(rowTransform, false);
            SR(toggleGO, 0.5f, 0.5f, 45, 45, 440, 0);

            var toggleBg = MakeImage(toggleGO.transform, "Background", new Color32(20, 25, 35, 255));
            StretchFull(toggleBg);
            var bgOutline = toggleBg.AddComponent<UnityEngine.UI.Outline>();
            bgOutline.effectColor = new Color32(50, 70, 95, 255);

            var toggleCheck = MakeImage(toggleGO.transform, "Checkmark", new Color32(220, 180, 50, 255));
            SR(toggleCheck, 0.5f, 0.5f, 28, 28, 0, 0);

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = toggleBg.GetComponent<Image>();
            toggle.graphic = toggleCheck.GetComponent<Image>();
            toggle.isOn = false;

            adToggles.Add(toggle);

            y += step;
        }
    }

    private GameObject MakeImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<Image>().color = color;
        return obj;
    }

    private GameObject MakeText(Transform parent, string name, string text, int size, Color color)
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

    private void SR(GameObject obj, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = obj.GetComponent<RectTransform>();
        if (!rt) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    private void StretchFull(GameObject obj)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private System.Collections.IEnumerator LoadNewspaperAndUnload()
    {
        var loadOp = SceneManager.LoadSceneAsync("NewspaperScene", LoadSceneMode.Additive);
        yield return loadOp;

        SceneManager.UnloadSceneAsync("DaySummaryScene");
    }

    public void OnNewspaperContinueClicked()
    {
        Debug.LogWarning("[DaySummarySceneManager] OnNewspaperContinueClicked jest przestarzałe — użyj NewspaperSceneManager.");
    }

    private string FormatDiff(float diff, string fmt = "F0", string suffix = "")
    {
        string sign = diff >= 0 ? "+" : "";
        return $"{sign}{diff.ToString(fmt)}{suffix}";
    }

    private Color DiffColor(float diff)
    {
        if (diff > 0) return new Color32(80, 220, 100, 255);
        if (diff < 0) return new Color32(220, 80, 80, 255);
        return new Color32(140, 140, 140, 255);
    }
}