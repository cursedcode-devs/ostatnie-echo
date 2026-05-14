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
        // Zatrzymujemy upływ czasu w grze
        Time.timeScale = 0f;
        
        BindDataToUI();

        if (summaryPanel != null) summaryPanel.SetActive(true);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (newspaperPanel != null) newspaperPanel.SetActive(false);

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

        if (newspaperContinueButton != null)
        {
            newspaperContinueButton.onClick.RemoveAllListeners();
            newspaperContinueButton.onClick.AddListener(OnNewspaperContinueClicked);
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
            moneyDiffText.color = DiffColor(DaySummaryData.MoneyDiff);
        }

        if (hipHopFinalText != null) hipHopFinalText.text = $"{DaySummaryData.HipHop}";
        if (discoFinalText != null) discoFinalText.text = $"{DaySummaryData.Disco}";
        if (rockFinalText != null) rockFinalText.text = $"{DaySummaryData.Rock}";
        if (popFinalText != null) popFinalText.text = $"{DaySummaryData.Pop}";

        if (hipHopDiffText != null)
        {
            hipHopDiffText.text = FormatDiff(DaySummaryData.HipHopDiff);
            hipHopDiffText.color = DiffColor(DaySummaryData.HipHopDiff);
        }
        if (discoDiffText != null)
        {
            discoDiffText.text = FormatDiff(DaySummaryData.DiscoDiff);
            discoDiffText.color = DiffColor(DaySummaryData.DiscoDiff);
        }
        if (rockDiffText != null)
        {
            rockDiffText.text = FormatDiff(DaySummaryData.RockDiff);
            rockDiffText.color = DiffColor(DaySummaryData.RockDiff);
        }
        if (popDiffText != null)
        {
            popDiffText.text = FormatDiff(DaySummaryData.PopDiff);
            popDiffText.color = DiffColor(DaySummaryData.PopDiff);
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
        }
    }

    public void OnShopContinueClicked()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (newspaperPanel != null) newspaperPanel.SetActive(true);
    }

    public void OnNewspaperContinueClicked()
    {
        Time.timeScale = 1f;

        DaySummaryData.OnSummaryClosed?.Invoke();
        DaySummaryData.OnSummaryClosed = null;

        SceneManager.UnloadSceneAsync("DaySummaryScene");
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
