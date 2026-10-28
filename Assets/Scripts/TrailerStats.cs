using UnityEngine;
using TMPro;

public class TrailerStats : MonoBehaviour
{
    public TextMeshProUGUI budgetText;
    
    public TextMeshProUGUI clockText;
    
    [Header("Bars Configuration")]
    public float maxBarHeight = 250f;
    public int currentMaxListeners = 500;

    [Header("Animation")]
    public float animationSpeed = 10f;
    public float animationDelay = 0.25f;

    [Header("Starting Values")]
    public float startingMoney = 1000f;
    public int startingPop = 100;
    public int startingRock = 100;
    public int startingHipHop = 100;
    public int startingDisco = 100;
    [Range(0, 23)]
    public int startingHour = 12;
    
    [Header("Clock Settings")]
    public float clockSpeed = 60f; // virtual minutes per second of game time

    [Header("Genre Bars")]
    public GenreBarUI popBar;
    public GenreBarUI rockBar;
    public GenreBarUI hipHopBar;
    public GenreBarUI discoBar;

    [Header("Input Settings")]
    public float moneyChangeAmount = 100f;
    public int popChangeAmount = 50;
    public int rockChangeAmount = 50;
    public int hipHopChangeAmount = 50;
    public int discoChangeAmount = 50;

    private float currentMoney;
    private float displayMoney;
    private Coroutine budgetAnimationRoutine;

    private int pop;
    private int rock;
    private int hipHop;
    private int disco;
    
    private int displayPop;
    private int displayRock;
    private int displayHipHop;
    private int displayDisco;
    
    private float currentClockMinutes;

    void Start()
    {
        currentMoney = startingMoney;
        displayMoney = startingMoney;
        pop = startingPop;
        rock = startingRock;
        hipHop = startingHipHop;
        disco = startingDisco;
        
        displayPop = pop;
        displayRock = rock;
        displayHipHop = hipHop;
        displayDisco = disco;
        
        currentClockMinutes = startingHour * 60f;
        UpdateUI();
    }

    private float autoChangeTimer = 0f;
    public float autoChangeInterval = 2f;
    public bool enableAutoChange = true;

    void Update()
    {
        HandleInput();
        UpdateUI();

        if (enableAutoChange)
        {
            autoChangeTimer += Time.deltaTime;
            if (autoChangeTimer >= autoChangeInterval)
            {
                autoChangeTimer = 0f;
                RandomizeStatsAdd();
            }
        }
    }

    private void RandomizeStatsAdd()
    {
        // random direction per stat
        int moneyMult = Random.value > 0.5f ? 1 : -1;
        currentMoney += moneyChangeAmount * moneyMult;
        
        pop = Mathf.Max(0, pop + popChangeAmount * (Random.value > 0.5f ? 1 : -1));
        rock = Mathf.Max(0, rock + rockChangeAmount * (Random.value > 0.5f ? 1 : -1));
        hipHop = Mathf.Max(0, hipHop + hipHopChangeAmount * (Random.value > 0.5f ? 1 : -1));
        disco = Mathf.Max(0, disco + discoChangeAmount * (Random.value > 0.5f ? 1 : -1));

        if (budgetAnimationRoutine != null) StopCoroutine(budgetAnimationRoutine);
        budgetAnimationRoutine = StartCoroutine(AnimateBudget(displayMoney, currentMoney));
    }

    private void HandleInput()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null) return;

        if (UnityEngine.InputSystem.Keyboard.current.equalsKey.wasPressedThisFrame || 
            UnityEngine.InputSystem.Keyboard.current.numpadPlusKey.wasPressedThisFrame)
        {
            AddStats(1);
        }
        else if (UnityEngine.InputSystem.Keyboard.current.minusKey.wasPressedThisFrame || 
                 UnityEngine.InputSystem.Keyboard.current.numpadMinusKey.wasPressedThisFrame)
        {
            AddStats(-1);
        }
    }

    private void AddStats(int multiplier)
    {
        currentMoney += moneyChangeAmount * multiplier;
        pop = Mathf.Max(0, pop + popChangeAmount * multiplier);
        rock = Mathf.Max(0, rock + rockChangeAmount * multiplier);
        hipHop = Mathf.Max(0, hipHop + hipHopChangeAmount * multiplier);
        disco = Mathf.Max(0, disco + discoChangeAmount * multiplier);

        if (budgetAnimationRoutine != null) StopCoroutine(budgetAnimationRoutine);
        budgetAnimationRoutine = StartCoroutine(AnimateBudget(displayMoney, currentMoney));
    }

    private System.Collections.IEnumerator AnimateBudget(float fromMoney, float toMoney)
    {
        yield return new WaitForSeconds(animationDelay);
        float elapsedTime = 0f;
        float duration = 2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            displayMoney = Mathf.Lerp(fromMoney, toMoney, elapsedTime / duration);
            yield return null;
        }

        displayMoney = toMoney;
        budgetAnimationRoutine = null;
    }

    private void UpdateUI()
    {
        if (budgetText != null)
        {
            budgetText.text = $"Budżet: {displayMoney:F0} zł";
        }

        if (clockText != null)
        {
            currentClockMinutes += Time.deltaTime * clockSpeed;
            int hours = Mathf.FloorToInt(currentClockMinutes / 60f) % 24;
            int minutes = Mathf.FloorToInt(currentClockMinutes % 60f);

            // blinking colon
            string colon = (Time.time % 1f < 0.5f) ? ":" : "<color=#00000000>:</color>";
            clockText.text = $"{hours:00}{colon}{minutes:00}";
        }

        UpdateBar(popBar, ref displayPop, pop, currentMaxListeners);
        UpdateBar(rockBar, ref displayRock, rock, currentMaxListeners);
        UpdateBar(hipHopBar, ref displayHipHop, hipHop, currentMaxListeners);
        UpdateBar(discoBar, ref displayDisco, disco, currentMaxListeners);
    }

    private void UpdateBar(GenreBarUI bar, ref int displayAmount, int targetAmount, int currentMax)
    {
        if (bar == null) return;
        
        displayAmount = (int)Mathf.MoveTowards(displayAmount, targetAmount, Time.deltaTime * animationSpeed * 50f);

        if (bar.amountText != null)
            bar.amountText.text = displayAmount.ToString();
            
        if (bar.barFill != null)
        {
            float fillRatio = Mathf.Clamp01((float)displayAmount / currentMax);
            float targetHeight = fillRatio * maxBarHeight;
            float currentHeight = bar.barFill.sizeDelta.y;
            float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * animationSpeed);
            
            bar.barFill.sizeDelta = new Vector2(bar.barFill.sizeDelta.x, newHeight);
        }
    }
}
