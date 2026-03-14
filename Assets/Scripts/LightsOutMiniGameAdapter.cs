using UnityEngine;
using System.Collections;

public class LightsOutMiniGameAdapter : BaseMiniGame
{
    [Header("Lights Out - ustawienia")]
    public int shuffleCount = 8;

    private LightsOutMiniGame game;
    private MiniGameSceneBuilder builder;
    private bool built = false;

    void Awake()
    {
        builder = GetComponent<MiniGameSceneBuilder>();
        if (builder != null) builder.buildOnStart = false;
    }

    protected override void OnLaunch()
    {
        if (!built) BuildScene();

        if (game != null)
        {
            game.shuffleCount = shuffleCount;
            game.StartGame();
        }
    }

    protected override void OnClose() { }

    void BuildScene()
    {
        if (builder == null)
        {
            builder = gameObject.AddComponent<MiniGameSceneBuilder>();
            builder.buildOnStart = false;
        }

        // Przekaż własny transform — canvas stanie się dzieckiem tego GameObject
        builder.Build(transform);

        // Po zbudowaniu canvas jest dzieckiem — znajdź go
        var canvasGO = transform.Find("MiniGameCanvas");
        if (canvasGO != null)
            miniGameCanvas = canvasGO.gameObject;
        else
        {
            // Fallback po nazwie w całej scenie
            var found = GameObject.Find("MiniGameCanvas");
            if (found != null) miniGameCanvas = found;
        }

        game = GetComponentInChildren<LightsOutMiniGame>(true);

        if (game != null)
            game.OnWin += OnLightsOutWon;
        else
            Debug.LogError("[LightsOutAdapter] Nie znaleziono LightsOutMiniGame!");

        built = true;
    }

    void OnLightsOutWon() => StartCoroutine(DelayedWin(1.5f));

    IEnumerator DelayedWin(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerWin();
    }
}