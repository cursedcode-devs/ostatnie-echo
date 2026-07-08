using UnityEngine;
using System.Collections;

/// <summary>
/// CableFixMiniGameAdapter
/// ========================
/// Mirrors LightsOutMiniGameAdapter exactly.
/// Wraps CableFixMiniGame (plain MonoBehaviour) into the BaseMiniGame system.
///
/// SETUP:
///   1. Create a prefab with this component + CableFixSceneBuilder on the same GameObject
///   2. Create a MiniGameDefinition SO pointing at this prefab
///      miniGameName = "CableFix"
///   3. Add the definition to MiniGameSystem.miniGames[]
/// </summary>
public class CableFixMiniGameAdapter : BaseMiniGame
{
    [Header("CableFix settings")]
    public float winDelay = 0.2f;

    private CableFixMiniGame game;
    private CableFixSceneBuilder builder;
    private bool built = false;

    // ------------------------------------------------------------------
    void Awake()
    {
        builder = GetComponent<CableFixSceneBuilder>();
        if (builder != null) builder.buildOnStart = false;
    }

    protected override void OnLaunch()
    {
        if (!built) BuildScene();
        game?.StartGame();
    }

    protected override void OnClose() { }

    // ------------------------------------------------------------------
    void BuildScene()
    {
        if (builder == null)
        {
            builder = gameObject.AddComponent<CableFixSceneBuilder>();
            builder.buildOnStart = false;
        }

        builder.Build(transform);

        var canvasGO = transform.Find("CableFixCanvas");
        if (canvasGO != null)
            miniGameCanvas = canvasGO.gameObject;
        else
        {
            var found = GameObject.Find("CableFixCanvas");
            if (found != null) miniGameCanvas = found;
        }

        game = GetComponentInChildren<CableFixMiniGame>(true);

        if (game != null)
            game.OnWin += OnCableFixWon;
        else
            Debug.LogError("[CableFixAdapter] CableFixMiniGame not found after BuildScene!");

        built = true;
    }

    void OnCableFixWon() => StartCoroutine(DelayedWin(winDelay));

    IEnumerator DelayedWin(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerWin();
    }
}
