using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Menedżer sceny podsumowującej dzień.
/// Odczytuje statystyki z DaySummaryData i przypisuje je do podpiętych elementów UI.
/// </summary>
public class DaySummarySceneManager : MonoBehaviour
{
    [Header("Główne")]
    public TextMeshProUGUI titleText;
    public Button continueButton;

    [Header("Koszty")]
    public TextMeshProUGUI rentFeeText;
    public TextMeshProUGUI foodFeeText;
    public TextMeshProUGUI studiesFeeText;

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
    public GameObject summaryPanel;
    public GameObject shopPanel;
    public GameObject newspaperPanel;

    [Header("Dodatkowe Przyciski")]
    public Button shopContinueButton;
    public Button newspaperContinueButton;

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

        if (summaryPanel != null) summaryPanel.SetActive(true);
        if (shopPanel != null) shopPanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnSummaryContinueClicked);
        }
        
        if (shopContinueButton != null)
        {
            shopContinueButton.onClick.RemoveAllListeners();
            shopContinueButton.onClick.AddListener(OnShopContinueClicked);
        }
    }

    void BindDataToUI()
    {
        if (titleText != null) titleText.text = $"KONIEC DNIA {DaySummaryData.Day - 1}";

        if (rentFeeText != null) rentFeeText.text = $"{DaySummaryData.RentFee:F2}$";
        if (foodFeeText != null) foodFeeText.text = $"{DaySummaryData.FoodFee:F2}$";
        if (studiesFeeText != null) studiesFeeText.text = $"{DaySummaryData.StudiesFee:F2}$";

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

    public void OnSummaryContinueClicked()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);
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

        // Pokaż draft ulepszeń, a dopiero po nim załaduj gazetę
        var upgradeManager = UnityEngine.Object.FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager != null)
        {
            upgradeManager.ShowDraftScreen(() =>
            {
                StartCoroutine(LoadNewspaperAndUnload());
            });
        }
        else
        {
            StartCoroutine(LoadNewspaperAndUnload());
        }
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
