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

        if (budgetPanel != null) budgetPanel.SetActive(true);
        if (listenersPanel != null) listenersPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

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
    }

    void BindDataToUI()
    {
        if (titleTexts != null)
        {
            foreach (var t in titleTexts)
            {
                if (t != null) t.text = $"KONIEC DNIA {DaySummaryData.Day - 1}";
            }
        }

        if (rentFeeText != null) rentFeeText.text = $"{DaySummaryData.RentFee:F2}$";
        if (foodFeeText != null) foodFeeText.text = $"{DaySummaryData.FoodFee:F2}$";
        if (studiesFeeText != null) studiesFeeText.text = $"{DaySummaryData.StudiesFee:F2}$";

        if (adsPenaltyText != null) adsPenaltyText.text = $"{DaySummaryData.AdsPenalty:F2}$";
        if (adsPenaltyBreakdownText != null)
        {
            if (DaySummaryData.UnplayedPenalties != null && DaySummaryData.UnplayedPenalties.Count > 0)
            {
                string bText = "Niewyemitowane zlecenia (kara 1/2 zysku):\n";
                foreach (var p in DaySummaryData.UnplayedPenalties)
                {
                    bText += $" • {p.clientName} (\"{p.adTitle}\"): -{p.penaltyAmount:F2}$\n";
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

        if (moneyFinalText != null) moneyFinalText.text = $"{DaySummaryData.FinalMoney:F2}$";
        if (moneyDiffText != null)
        {
            moneyDiffText.text = FormatDiff(DaySummaryData.MoneyDiff, "F2", "$");
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
            // popDiffText.color = DiffColor(DaySummaryData.PopDiff);
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
                        if (price != null) price.text = $"{(i + 1) * 15}$";
                        if (stats != null) stats.text = "+10 Zadowolenia";
                    }
                }
#endif
            }
        }
    }

    public void OnShopContinueClicked()
    {
        if (shopPanel != null) shopPanel.SetActive(false);

        if (contractsPanel != null)
        {
            contractsPanel.SetActive(true);
            BuildContractsUI();
        }
        else
        {
            StartCoroutine(LoadNewspaperAndUnload());
        }
    }

    public void OnContractsContinueClicked()
    {
        if (contractsPanel != null) contractsPanel.SetActive(false);

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

        StartCoroutine(LoadNewspaperAndUnload());
    }

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
            SR(rowBox, 0.5f, 1f, 1020, 95, 0, -y - 50); // Zmiana pivota na górę lub odpowiednie ułożenie
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

            var payoutTxt = MakeText(payoutTag.transform, "PayoutText", $"EST. ZAROBEK: {payout:F2}$", 16, new Color32(80, 220, 100, 255));
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
