using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// LIGHTS OUT - Panel Sterowania (Wersja Minigry)
/// =================================================
/// Plansza 4x4. Cztery przełączniki narożne (żółty, zielony, niebieski, fioletowy).
/// Każdy przełącznik zmienia obszar 3x3 w swoim rogu planszy.
///
///   Żółty (lewy górny)   │  Zielony (prawy górny)
///   Niebieski (lewy dolny)│  Fioletowy (prawy dolny)
///
/// Mapa wpływu (0-indexed, wiersz×kolumna):
///   Żółty     = wiersze 0-2, kolumny 0-2
///   Zielony   = wiersze 0-2, kolumny 1-3
///   Niebieski = wiersze 1-3, kolumny 0-2
///   Fioletowy = wiersze 1-3, kolumny 1-3
///
/// SETUP:
///   GameManager (LightsOutMiniGame)
///   Canvas
///     ├── LightsGrid  (GridLayoutGroup 4x4, 16 dzieci Image/Button)
///     ├── SwitchYellow   (Button + SwitchLever)
///     ├── SwitchGreen    (Button + SwitchLever)
///     ├── SwitchBlue     (Button + SwitchLever)
///     ├── SwitchPurple   (Button + SwitchLever)
///     ├── StatusText     (TextMeshProUGUI)
///     ├── MovesText      (TextMeshProUGUI)
///     └── RestartButton  (Button)
///
/// Przypisz lightImages[0..15] = dzieci LightsGrid w kolejności
/// wierszami od lewego-górnego.
/// </summary>
public class LightsOutMiniGame : MonoBehaviour
{
    // ------------------------------------------------------------------
    //  Stałe
    // ------------------------------------------------------------------
    public const int ROWS = 4;
    public const int COLS = 4;

    /// <summary>
    /// Definicja czterech przełączników: który zestaw komórek togglują.
    /// Każdy wpis to lista (row, col) komórek zmienianych przez ten przełącznik.
    /// </summary>
    // Pary [row, col] zakodowane flat: indeks i=row, i+1=col
    static readonly int[][] SwitchRegions = new int[][]
    {
        new int[] { 0,0, 0,1, 0,2,  1,0, 1,1, 1,2,  2,0, 2,1, 2,2 }, // Zolty
        new int[] { 0,1, 0,2, 0,3,  1,1, 1,2, 1,3,  2,1, 2,2, 2,3 }, // Zielony
        new int[] { 1,0, 1,1, 1,2,  2,0, 2,1, 2,2,  3,0, 3,1, 3,2 }, // Niebieski
        new int[] { 1,1, 1,2, 1,3,  2,1, 2,2, 2,3,  3,1, 3,2, 3,3 }, // Fioletowy
    };

    // ------------------------------------------------------------------
    //  Inspektor
    // ------------------------------------------------------------------
    [Header("Światła (16 elementów, wierszami od lewego-górnego)")]
    public Image[] lightImages = new Image[ROWS * COLS];

    [Header("Przełączniki")]
    public Button switchYellow;
    public Button switchGreen;
    public Button switchBlue;
    public Button switchPurple;

    [Header("UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI movesText;
    public Button restartButton;
    [Header("SFX")]
    public FMODUnity.EventReference clickSound;
    [Header("Kolory świateł")]
    public Color lightOnColor  = new Color(1f, 0.95f, 0.3f);
    public Color lightOffColor = new Color(0.15f, 0.12f, 0.1f);

    [Header("Tasowanie")]
    [Tooltip("Ile losowych kliknięć przy starcie")]
    public int shuffleCount = 6;

    // ------------------------------------------------------------------
    //  Zdarzenia
    // ------------------------------------------------------------------

    /// <summary>Wywoływane gdy gracz zapali wszystkie lampki.</summary>
    public event System.Action OnWin;

    // ------------------------------------------------------------------
    //  Stan
    // ------------------------------------------------------------------
    private bool[] lights = new bool[ROWS * COLS];  // true = zapalone
    private int moves = 0;
    private bool won = false;
    private bool busy = false;

    // Referencje do SwitchLever (opcjonalne - animacja)
    private SwitchLever[] levers = new SwitchLever[4];

    // ------------------------------------------------------------------
    // Start() NIE podpina listenerów — robi to Initialize() wywoływane
    // przez MiniGameSceneBuilder zaraz po zbudowaniu UI.
    void Start() { }

    /// <summary>
    /// Wywołaj po przypisaniu wszystkich referencji (switchYellow itd.).
    /// MiniGameSceneBuilder robi to automatycznie.
    /// </summary>
    public void Initialize()
    {
        // Odepnij stare listenery żeby nie duplikować przy restarcie
        Button[] btns = { switchYellow, switchGreen, switchBlue, switchPurple };
        for (int i = 0; i < 4; i++)
        {
            if (btns[i] == null) continue;
            btns[i].onClick.RemoveAllListeners();
            levers[i] = btns[i].GetComponent<SwitchLever>();
            int idx = i;
            btns[i].onClick.AddListener(() => OnSwitch(idx));
        }

        if (restartButton)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(StartGame);
        }

        StartGame();
    }

    // ------------------------------------------------------------------
    public void StartGame()
    {
        StopAllCoroutines();
        busy = false;
        won  = false;
        moves = 0;

        // Zapal wszystkie
        for (int i = 0; i < lights.Length; i++) lights[i] = true;

        // Tasuj przez losowe kliknięcia przełączników
        for (int s = 0; s < shuffleCount; s++)
            ApplyRegion(Random.Range(0, 4));

        // Upewnij się że nie jest wygrane od razu
        if (IsWon()) StartGame();

        RefreshLights();
        RefreshHUD();
    }

    // ------------------------------------------------------------------
    void OnSwitch(int switchIdx)
    {
        FMODUnity.RuntimeManager.PlayOneShot(clickSound, this.transform.position);
        if (won || busy) return;

        // Animacja dźwigni
        if (levers[switchIdx] != null)
            levers[switchIdx].TriggerSwitch();

        ApplyRegion(switchIdx);
        moves++;
        RefreshLights();
        RefreshHUD();

        if (IsWon()) StartCoroutine(WinSequence());
    }

    // ------------------------------------------------------------------
    void ApplyRegion(int switchIdx)
    {
        int[] region = SwitchRegions[switchIdx];
        for (int i = 0; i < region.Length; i += 2)
        {
            int r = region[i];
            int c = region[i + 1];
            lights[r * COLS + c] = !lights[r * COLS + c];
        }
    }

    // ------------------------------------------------------------------
    bool IsWon()
    {
        foreach (bool b in lights)
            if (!b) return false;
        return true;
    }

    // ------------------------------------------------------------------
    void RefreshLights()
    {
        for (int i = 0; i < lights.Length && i < lightImages.Length; i++)
        {
            if (lightImages[i] == null) continue;
            lightImages[i].color = lights[i] ? lightOnColor : lightOffColor;

            // GlowEffect (opcjonalny)
            var glow = lightImages[i].GetComponentInChildren<GlowEffect>(true);
            if (glow) glow.SetActive(lights[i]);
        }
    }

    // ------------------------------------------------------------------
    void RefreshHUD()
    {
        if (movesText) movesText.text = $"RUCHY: {moves}";

        if (statusText && !won)
        {
            int off = 0;
            foreach (bool b in lights) if (!b) off++;
            statusText.text  = off == 0 ? "GOTOWE" : $"WYŁĄCZONYCH: {off}";
            statusText.color = off == 0 ? lightOnColor : Color.white;
        }
    }

    // ------------------------------------------------------------------
    IEnumerator WinSequence()
    {
        won = true;
        busy = true;

        OnWin?.Invoke();

        if (statusText)
        {
            statusText.text  = "SYSTEMY AKTYWNE!";
            statusText.color = lightOnColor;
        }

        // Migotanie
        for (int f = 0; f < 6; f++)
        {
            SetAllLightsColor(Color.white);
            yield return new WaitForSeconds(0.08f);
            SetAllLightsColor(lightOnColor);
            yield return new WaitForSeconds(0.08f);
        }

        busy = false;
    }

    void SetAllLightsColor(Color c)
    {
        foreach (var img in lightImages)
            if (img) img.color = c;
    }

    // ------------------------------------------------------------------
    // Dostępne z zewnątrz (np. z menu trudności)
    // ------------------------------------------------------------------

    /// <summary>Zmień trudność i zrestartuj.</summary>
    public void SetDifficulty(int shuffles)
    {
        shuffleCount = Mathf.Max(1, shuffles);
        StartGame();
    }

    /// <summary>
    /// Tryb "uszkodzonych świateł" - losowe lampki wyglądają inaczej
    /// (np. migają mimo że są "on") aby zmylić gracza.
    /// Przekaż tablicę indeksów lampek do "uszkodzenia".
    /// </summary>
    public void SetBrokenLights(int[] brokenIndices)
    {
        // Implementacja: nadaj inny kolor/animację tym lampkom
        // Pozostaw logikę bez zmian - gracz musi domyśleć się co działa
        foreach (int idx in brokenIndices)
        {
            if (idx < 0 || idx >= lightImages.Length) continue;
            var img = lightImages[idx];
            if (img) img.color = new Color(0.4f, 0.15f, 0.1f); // czerwonawe - podejrzane
        }
    }
}