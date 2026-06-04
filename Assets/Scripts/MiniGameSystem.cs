using UnityEngine;
using UnityEngine.InputSystem;
using Key = UnityEngine.InputSystem.Key;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// MiniGameSystem — centralny manager wszystkich minigier.
///
/// SETUP W UNITY:
///   1. Utwórz GameObject "MiniGameSystem", dodaj ten skrypt
///   2. Przypisz globalRewardPool[]
///   3. Przypisz miniGames[] — definicje minigier
///   4. Przypisz amplitudeSlider, lengthSlider, frequencySlider ze sceny
///
/// WYWOŁANIE:
///   MiniGameSystem.Instance.Launch("WaveTweaking");
///   MiniGameSystem.Instance.Launch("LightsOut");
///   MiniGameSystem.Instance.LaunchRandom();
/// </summary>
public class MiniGameSystem : MonoBehaviour
{
    public static MiniGameSystem Instance { get; private set; }

    // ------------------------------------------------------------------
    [Header("Referencje do gry")]
    public RadioStation radioStation;
    public DayEndHandler dayEndHandler;
    public GameManager gameManager;

    [Header("Globalna pula nagród")]
    public MiniGameReward[] globalRewardPool;

    [Header("Definicje minigier")]
    public MiniGameDefinition[] miniGames;

    [Header("Wave Tweaking — suwaki ze sceny")]
    [Tooltip("Przeciągnij tu Length/Amplitude/Frequency ze sceny.")]
    public ConsoleSliderObject amplitudeSlider;
    public ConsoleSliderObject lengthSlider;
    public ConsoleSliderObject frequencySlider;

    [Header("UI - Popup nagrody (opcjonalny)")]
    public GameObject rewardPopupCanvas;
    public TextMeshProUGUI rewardText;
    public float popupDuration = 3f;

    [Header("Ustawienia")]
    public Key closeKey = Key.Escape;
    public bool disableClicksWhenOpen = true;

    // ------------------------------------------------------------------
    private BaseMiniGame currentMiniGame;
    private MiniGameDefinition currentDefinition;
    private Dictionary<string, BaseMiniGame> spawnedInstances = new();
    public FMODUnity.EventReference successSound;
    // ------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (gameManager == null)   gameManager   = FindFirstObjectByType<GameManager>();
        if (dayEndHandler == null) dayEndHandler = FindFirstObjectByType<DayEndHandler>();
        if (gameManager != null)   radioStation  = gameManager.radioStation;

        if (rewardPopupCanvas != null)
            rewardPopupCanvas.SetActive(false);

        foreach (var def in miniGames)
        {
            if (def == null || def.prefab == null) continue;
            SpawnMiniGame(def);
        }
    }

    void Update()
    {
        if (currentMiniGame != null && currentMiniGame.IsOpen)
            if (Keyboard.current[closeKey].wasPressedThisFrame)
                CloseCurrent();
    }

    // ------------------------------------------------------------------
    public void Launch(string miniGameName)
    {
        var def = FindDefinition(miniGameName);
        if (def == null) { Debug.LogWarning($"[MiniGameSystem] Nie znaleziono: '{miniGameName}'"); return; }
        LaunchDefinition(def);
    }

    public void LaunchRandom()
    {
        if (miniGames == null || miniGames.Length == 0) return;
        LaunchDefinition(miniGames[Random.Range(0, miniGames.Length)]);
    }

    public void CloseCurrent() => currentMiniGame?.Close();

    public bool IsAnyOpen => currentMiniGame != null && currentMiniGame.IsOpen;

    // ------------------------------------------------------------------
    void LaunchDefinition(MiniGameDefinition def)
    {
        if (IsAnyOpen) return;

        if (!spawnedInstances.TryGetValue(def.miniGameName, out BaseMiniGame instance))
            instance = SpawnMiniGame(def);

        currentMiniGame   = instance;
        currentDefinition = def;

        currentMiniGame.OnWon    -= HandleWin;
        currentMiniGame.OnClosed -= HandleClosed;
        currentMiniGame.OnWon    += HandleWin;
        currentMiniGame.OnClosed += HandleClosed;

        if (disableClicksWhenOpen && def.disableClicksWhenOpen && gameManager != null)
            gameManager.SetInputEnabled(false);

        currentMiniGame.Launch();
    }

    BaseMiniGame SpawnMiniGame(MiniGameDefinition def)
    {
        string expectedName = $"MiniGame_{def.miniGameName}";
        BaseMiniGame instance = null;

        // Szukaj istniejącego obiektu na scenie (w tym nieaktywnego)
        BaseMiniGame[] allMiniGames = Resources.FindObjectsOfTypeAll<BaseMiniGame>();
        foreach (var mg in allMiniGames)
        {
            if (mg.gameObject.scene.isLoaded && mg.gameObject.name == expectedName)
            {
                instance = mg;
                break;
            }
        }

        GameObject go;
        if (instance != null)
        {
            go = instance.gameObject;
        }
        else if (def.prefab != null)
        {
            go = Instantiate(def.prefab);
            go.name = expectedName;
            instance = go.GetComponent<BaseMiniGame>();
        }
        else
        {
            Debug.LogError($"[MiniGameSystem] Brak prefabu i obiektu na scenie dla {def.miniGameName}");
            return null;
        }

        if (instance == null)
        {
            Debug.LogError($"[MiniGameSystem] Prefab '{def.prefab.name}' nie ma BaseMiniGame!");
            Destroy(go);
            return null;
        }

        // Inject sliders into WaveTweakingMiniGameAdapter
        // (adapter is the BaseMiniGame, not WaveTweakingMiniGame directly)
        var waveTweakingAdapter = go.GetComponent<WaveTweakingMiniGameAdapter>();
        if (waveTweakingAdapter != null)
        {
            waveTweakingAdapter.SetSliders(amplitudeSlider, lengthSlider, frequencySlider);

            if (amplitudeSlider == null || lengthSlider == null || frequencySlider == null)
                Debug.LogWarning("[MiniGameSystem] One or more WaveTweaking sliders not assigned!");
        }

        if (instance.miniGameCanvas != null)
            instance.miniGameCanvas.SetActive(false);

        spawnedInstances[def.miniGameName] = instance;
        return instance;
    }

    void HandleWin()
    {
        // Capture definition locally — HandleClosed may null currentDefinition synchronously
        MiniGameDefinition wonDefinition = currentDefinition;

        if (wonDefinition == null)
        {
            Debug.LogWarning("[MiniGameSystem] HandleWin fired but currentDefinition was null — rewards skipped.");
            return;
        }
        FMODUnity.RuntimeManager.PlayOneShot(successSound, this.transform.position);
        var rewards = wonDefinition.DrawRewards(globalRewardPool);
        ApplyRewards(rewards);

        if (rewards.Length > 0)
            StartCoroutine(ShowRewardPopup(rewards));

        StartCoroutine(CloseAfterDelay(popupDuration + 0.5f));
    }

    void HandleClosed()
    {
        if (disableClicksWhenOpen && currentDefinition != null && currentDefinition.disableClicksWhenOpen && gameManager != null)
            gameManager.SetInputEnabled(true);

        currentMiniGame   = null;
        currentDefinition = null;
    }

    void ApplyRewards(MiniGameReward[] rewards)
    {
        foreach (var r in rewards)
            r?.Apply(radioStation, dayEndHandler);
    }

public void ShowPopup(string title, string description)
{
    if (rewardPopupCanvas == null || rewardText == null) return;
    StartCoroutine(ShowPopupCoroutine(title, description));
}

private IEnumerator ShowPopupCoroutine(string title, string description)
{
    rewardText.text = $"{title}\n{description}";
    rewardPopupCanvas.SetActive(true);
    yield return new WaitForSeconds(popupDuration);
    rewardPopupCanvas.SetActive(false);
}


    IEnumerator ShowRewardPopup(MiniGameReward[] rewards)
    {
        if (rewardPopupCanvas == null || rewardText == null) yield break;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("NAGRODA!");
        foreach (var r in rewards)
        {
            if (r == null) continue;
            sb.AppendLine($"• {r.rewardName}");
            if (!string.IsNullOrEmpty(r.description))
                sb.AppendLine($"  {r.description}");
        }

        rewardText.text = sb.ToString();
        rewardPopupCanvas.SetActive(true);
        yield return new WaitForSeconds(popupDuration);
        rewardPopupCanvas.SetActive(false);
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseCurrent();
    }

    MiniGameDefinition FindDefinition(string name)
    {
        foreach (var def in miniGames)
            if (def != null && def.miniGameName == name) return def;
        return null;
    }
}