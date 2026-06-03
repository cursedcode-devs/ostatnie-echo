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
    [Tooltip("Opóźnienie w sekundach przed każdą sekwencją animacji.")]
    public float animationDelay = 0.25f;

    [Header("Genre Bars")]
    public GenreBarUI popBar;
    public GenreBarUI rockBar;
    public GenreBarUI hipHopBar;
    public GenreBarUI discoBar;

    private RadioStation radioStation;
    
    private GenreValues targetListeners;
    private GenreValues lastKnownListeners;
    private Coroutine animationRoutine;

    public void Initialize(RadioStation rs)
    {
        radioStation = rs;
        targetListeners = rs.currentListeners;
        lastKnownListeners = rs.currentListeners;
    }

    void Update()
    {
        if (radioStation == null) return;

        if (HasChanged(lastKnownListeners, radioStation.currentListeners))
        {
            if (animationRoutine != null) StopCoroutine(animationRoutine);
            animationRoutine = StartCoroutine(AnimateChangesSequentially(targetListeners, radioStation.currentListeners));
            lastKnownListeners = radioStation.currentListeners;
        }

        UpdateUI();
    }

    private bool HasChanged(GenreValues a, GenreValues b)
    {
        return a.pop != b.pop || a.rock != b.rock || a.hipHop != b.hipHop || a.disco != b.disco;
    }

    private System.Collections.IEnumerator AnimateChangesSequentially(GenreValues oldValues, GenreValues newValues)
    {
        yield return new WaitForSeconds(animationDelay);
        targetListeners = newValues;
    }

    public void UpdateUI()
    {
        budgetText.text = $"BUDŻET: {radioStation.GetCurrentMoney():F0} zł";

        int pop = targetListeners.pop;
        int rock = targetListeners.rock;
        int hipHop = targetListeners.hipHop;
        int disco = targetListeners.disco;

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