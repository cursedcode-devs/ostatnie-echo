using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    public TextMeshProUGUI budgetText;
    public TextMeshProUGUI clockText;
    
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
    private TimeHandler timeHandler;
    
    private GenreValues targetListeners;
    private GenreValues lastKnownListeners;
    private Coroutine animationRoutine;
    
    private int lastKnownHour = -1;
    private Coroutine clockAnimationRoutine;
    
    private float lastKnownMoney = -1f;
    private Coroutine budgetAnimationRoutine;
    private float currentDisplayMoney;
    [SerializeField] private TextMeshProUGUI dayText;

    public void Initialize(RadioStation rs, TimeHandler th)
    {
        radioStation = rs;
        timeHandler = th;
        targetListeners = rs.currentListeners;
        lastKnownListeners = rs.currentListeners;
        
        if (timeHandler != null)
            lastKnownHour = timeHandler.CurrentHour;
            
        if (radioStation != null)
        {
            lastKnownMoney = radioStation.GetCurrentMoney();
            currentDisplayMoney = lastKnownMoney;
        }
        
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

        if (timeHandler != null && lastKnownHour != timeHandler.CurrentHour)
        {
            if (clockAnimationRoutine != null) StopCoroutine(clockAnimationRoutine);
            clockAnimationRoutine = StartCoroutine(AnimateClock(lastKnownHour, timeHandler.CurrentHour));
            lastKnownHour = timeHandler.CurrentHour;
        }

        if (radioStation.GetCurrentMoney() != lastKnownMoney)
        {
            if (budgetAnimationRoutine != null) StopCoroutine(budgetAnimationRoutine);
            budgetAnimationRoutine = StartCoroutine(AnimateBudget(currentDisplayMoney, radioStation.GetCurrentMoney()));
            lastKnownMoney = radioStation.GetCurrentMoney();
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

    private System.Collections.IEnumerator AnimateClock(int fromHour, int toHour)
    {
        float currentMinutes = 0f;
        while (currentMinutes < 60f)
        {
            currentMinutes += Time.deltaTime * 30f;
            if (currentMinutes > 59f) currentMinutes = 59f;

            if (clockText != null)
            {
                string colon = (Time.time % 2f < 1f) ? ":" : "<color=#00000000>:</color>";
                clockText.text = $"{fromHour}{colon}{(int)currentMinutes:D2}";
            }

            if (currentMinutes >= 59f) break;

            yield return null;
        }

        if (clockText != null)
        {
            clockText.text = $"{toHour}:00";
        }

        clockAnimationRoutine = null;
    }

    private System.Collections.IEnumerator AnimateBudget(float fromMoney, float toMoney)
    {
        yield return new WaitForSeconds(animationDelay);
        float elapsedTime = 0f;
        float duration = 2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentDisplayMoney = Mathf.Lerp(fromMoney, toMoney, elapsedTime / duration);
            yield return null;
        }

        currentDisplayMoney = toMoney;
        budgetAnimationRoutine = null;
    }

    public void UpdateUI()
    {
        if (budgetAnimationRoutine == null && radioStation != null)
        {
            currentDisplayMoney = radioStation.GetCurrentMoney();
        }
        
        budgetText.text = $"Budżet: {currentDisplayMoney:F0} zł";
        
        if (clockText != null && timeHandler != null && clockAnimationRoutine == null)
        {
            string colon = (Time.time % 2f < 1f) ? ":" : "<color=#00000000>:</color>";
            clockText.text = $"{timeHandler.CurrentHour}{colon}00";
        }

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
        dayText.text = string.Format("DZIEŃ: {0} / 3", timeHandler.getDay());

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

[System.Serializable]
public class GenreBarUI
{
    public TextMeshProUGUI amountText;
    public RectTransform barFill;
}