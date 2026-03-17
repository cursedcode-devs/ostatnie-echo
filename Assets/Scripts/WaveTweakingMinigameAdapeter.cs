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

    private WaveTweakingMiniGame game;
    private WaveTweakingSceneBuilder builder;
    private bool built = false;

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

        if (game != null)
        {
            game.amplitudeSlider = amplitudeSlider;
            game.lengthSlider    = lengthSlider;
            game.frequencySlider = frequencySlider;
            game.StartGame();
        }
    }

    protected override void OnClose()
    {
        // Nothing extra — canvas hide is handled by BaseMiniGame
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