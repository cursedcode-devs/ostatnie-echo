using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PlaceholderMiniGame — szablon dla nowych minigier.
/// 
/// Żeby dodać nową minigre:
///   1. Zduplikuj ten plik, zmień nazwę
///   2. Zaimplementuj OnLaunch() i OnClose()
///   3. Wywołaj TriggerWin() gdy gracz wygra
///   4. Utwórz prefab, utwórz MiniGameDefinition, dodaj do MiniGameSystem
/// </summary>
public class PlaceholderMiniGame : BaseMiniGame
{
    [Header("Placeholder UI")]
    public TextMeshProUGUI titleText;
    public Button winButton;   // tymczasowy przycisk do testów
    public Button closeButton;

    // ------------------------------------------------------------------
    void Start()
    {
        if (winButton)   winButton.onClick.AddListener(TriggerWin);
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    protected override void OnLaunch()
    {
        Debug.Log($"[{GetType().Name}] Minigra uruchomiona!");
        if (titleText) titleText.text = GetType().Name;
        // TODO: zaimplementuj logikę minigry
    }

    protected override void OnClose()
    {
        Debug.Log($"[{GetType().Name}] Minigra zamknięta.");
        // TODO: posprzątaj stan
    }
}
