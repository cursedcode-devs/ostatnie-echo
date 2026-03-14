using UnityEngine;
using System;

/// <summary>
/// Interfejs który musi implementować każda minigra.
/// 
/// Dodając nową minigre:
///   1. Utwórz skrypt dziedziczący z BaseMiniGame
///   2. Zaimplementuj Launch() i Close()
///   3. Wywołaj OnWon() gdy gracz wygra
///   Gotowe — MiniGameSystem zajmie się resztą.
/// </summary>
public interface IMiniGame
{
    event Action OnWon;
    event Action OnClosed;

    void Launch();
    void Close();
    bool IsOpen { get; }
}

// ------------------------------------------------------------------

/// <summary>
/// Klasa bazowa dla wszystkich minigier.
/// Dziedź z tej klasy zamiast MonoBehaviour.
/// </summary>
public abstract class BaseMiniGame : MonoBehaviour, IMiniGame
{
    public event Action OnWon;
    public event Action OnClosed;

    public bool IsOpen { get; private set; }

    [Header("Minigra - ustawienia bazowe")]
    public GameObject miniGameCanvas;

    // ------------------------------------------------------------------

    public virtual void Launch()
    {
        if (IsOpen) return;
        IsOpen = true;

        if (miniGameCanvas != null)
            miniGameCanvas.SetActive(true);

        OnLaunch();
    }

    public virtual void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (miniGameCanvas != null)
            miniGameCanvas.SetActive(false);

        OnClose();
        OnClosed?.Invoke();
    }

    // ------------------------------------------------------------------
    // Metody do nadpisania w konkretnej minigrze

    /// <summary>Logika uruchamiania - nadpisz w subklasie.</summary>
    protected abstract void OnLaunch();

    /// <summary>Logika zamykania - nadpisz w subklasie.</summary>
    protected virtual void OnClose() { }

    // ------------------------------------------------------------------
    /// <summary>Wywołaj tę metodę gdy gracz wygra.</summary>
    protected void TriggerWin()
    {
        OnWon?.Invoke();
    }
}
