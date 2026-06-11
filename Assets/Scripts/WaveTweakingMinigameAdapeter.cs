using UnityEngine;
using System.Collections;

/// <summary>
/// WaveTweakingMiniGameAdapter
/// ============================
/// Mirrors LightsOutMiniGameAdapter exactly.
/// Wraps WaveTweakingMiniGame (plain MonoBehaviour) into the BaseMiniGame system.
///
/// SETUP:
///   1. Create a prefab with this component
///   2. Also add WaveTweakingSceneBuilder to the same GameObject
///   3. Assign amplitudeSlider, lengthSlider, frequencySlider in MiniGameSystem inspector
///      (MiniGameSystem.SpawnMiniGame calls SetSliders() on this adapter after instantiation)
///   4. Create a MiniGameDefinition SO pointing at this prefab
///   5. Add the definition to MiniGameSystem.miniGames[]
/// </summary>
public class WaveTweakingMiniGameAdapter : BaseMiniGame
{
    [Header("Wave Tweaking settings")]
    public float winDelay = 1.5f;

    [Header("Kamera (przybliżenie podczas minigry)")]
    [Tooltip("Nazwa obiektu-pozycji kamery, do której przybliżamy na czas minigry.")]
    public string cameraTargetName = "Pos_konsoleta";

    private WaveTweakingMiniGame game;
    private WaveTweakingSceneBuilder builder;
    private bool built = false;

    private ZoomHandler zoomHandler;
    private Transform cameraTarget;

    // Sliders injected by MiniGameSystem (scene objects can't live in prefab)
    private ConsoleSliderObject amplitudeSlider;
    private ConsoleSliderObject lengthSlider;
    private ConsoleSliderObject frequencySlider;

    // ------------------------------------------------------------------
    void Awake()
    {
        builder = GetComponent<WaveTweakingSceneBuilder>();
        if (builder != null) builder.buildOnStart = false;
    }

    /// <summary>Called by MiniGameSystem after instantiation.</summary>
    public void SetSliders(ConsoleSliderObject amplitude, ConsoleSliderObject length, ConsoleSliderObject frequency)
    {
        amplitudeSlider = amplitude;
        lengthSlider    = length;
        frequencySlider = frequency;

        // If already built, update the game directly
        if (game != null)
        {
            game.amplitudeSlider = amplitudeSlider;
            game.lengthSlider    = lengthSlider;
            game.frequencySlider = frequencySlider;
        }
    }

    // ------------------------------------------------------------------
    protected override void OnLaunch()
    {
        if (!built) BuildScene();

        // Przybliż kamerę do konsolety na czas minigry.
        EnsureZoomRefs();
        if (zoomHandler != null && cameraTarget != null)
            zoomHandler.ZoomToTransform(cameraTarget);

        if (game != null)
        {
            game.amplitudeSlider = amplitudeSlider;
            game.lengthSlider    = lengthSlider;
            game.frequencySlider = frequencySlider;
            game.StartGame();
        }

        // Wyróżnij suwaki, którymi gracz steruje osiami.
        SetSlidersHighlighted(true);
    }

    protected override void OnClose()
    {
        // Oddal kamerę z powrotem po zamknięciu minigry.
        if (zoomHandler != null)
            zoomHandler.ZoomOut();

        SetSlidersHighlighted(false);
    }

    void SetSlidersHighlighted(bool on)
    {
        SetHighlight(amplitudeSlider, on);
        SetHighlight(lengthSlider, on);
        SetHighlight(frequencySlider, on);
    }

    void SetHighlight(ConsoleSliderObject slider, bool on)
    {
        if (slider == null) return;
        var h = slider.GetComponent<SliderHighlighter>();
        if (h == null) h = slider.gameObject.AddComponent<SliderHighlighter>();
        h.enabled = on;
    }

    void EnsureZoomRefs()
    {
        if (zoomHandler == null)
            zoomHandler = FindFirstObjectByType<ZoomHandler>();

        if (cameraTarget == null && !string.IsNullOrEmpty(cameraTargetName))
        {
            var go = GameObject.Find(cameraTargetName);
            if (go != null) cameraTarget = go.transform;
            else Debug.LogWarning($"[WaveTweakingAdapter] Nie znaleziono obiektu kamery '{cameraTargetName}'.");
        }
    }

    // ------------------------------------------------------------------
    void BuildScene()
    {
        if (builder == null)
        {
            builder = gameObject.AddComponent<WaveTweakingSceneBuilder>();
            builder.buildOnStart = false;
        }

        builder.Build(transform);

        // Find the canvas that was built as a child
        var canvasGO = transform.Find("WaveTweakingCanvas");
        if (canvasGO != null)
            miniGameCanvas = canvasGO.gameObject;
        else
        {
            var found = GameObject.Find("WaveTweakingCanvas");
            if (found != null) miniGameCanvas = found;
        }

        game = GetComponentInChildren<WaveTweakingMiniGame>(true);

        if (game != null)
            game.OnWin += OnWaveTweakingWon;
        else
            Debug.LogError("[WaveTweakingAdapter] WaveTweakingMiniGame not found after BuildScene!");

        built = true;
    }

    void OnWaveTweakingWon() => StartCoroutine(DelayedWin(winDelay));

    IEnumerator DelayedWin(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerWin();
    }
}