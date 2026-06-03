using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class GenreBarUI
{
    public TextMeshProUGUI amountText;
    public RectTransform barFill;
}

public class StatsUI : MonoBehaviour
{
    public TextMeshProUGUI budgetText;
    
    [Header("Bars Configuration")]
    public float maxBarHeight = 250f;
    [Tooltip("If true, max listeners scales automatically to the highest current listener count.")]
    public bool dynamicMaxListeners = true;
    public int manualMaxListeners = 500;

    [Header("Animation")]
    public float animationSpeed = 10f;

    [Header("Genre Bars")]
    public GenreBarUI popBar;
    public GenreBarUI rockBar;
    public GenreBarUI hipHopBar;
    public GenreBarUI discoBar;

    private RadioStation radioStation;

    public void Initialize(RadioStation rs)
    {
        radioStation = rs;
    }

    void Update()
    {
        if (radioStation == null) return;
        UpdateUI();
    }

    public void UpdateUI()
    {
        budgetText.text = $"BUDŻET: {radioStation.GetCurrentMoney():F0} zł";

        int pop = radioStation.currentListeners.pop;
        int rock = radioStation.currentListeners.rock;
        int hipHop = radioStation.currentListeners.hipHop;
        int disco = radioStation.currentListeners.disco;

        int currentMax = manualMaxListeners;
        if (dynamicMaxListeners)
        {
            currentMax = Mathf.Max(pop, rock, hipHop, disco);
            if (currentMax < 10) currentMax = 10; // Prevent division by zero
        }

        UpdateBar(popBar, pop, currentMax);
        UpdateBar(rockBar, rock, currentMax);
        UpdateBar(hipHopBar, hipHop, currentMax);
        UpdateBar(discoBar, disco, currentMax);
    }

    private void UpdateBar(GenreBarUI bar, int amount, int currentMax)
    {
        if (bar.amountText != null)
            bar.amountText.text = amount.ToString();
            
        if (bar.barFill != null)
        {
            float fillRatio = Mathf.Clamp01((float)amount / currentMax);
            float targetHeight = fillRatio * maxBarHeight;
            float currentHeight = bar.barFill.sizeDelta.y;
            float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * animationSpeed);
            
            bar.barFill.sizeDelta = new Vector2(bar.barFill.sizeDelta.x, newHeight);
        }
    }
}