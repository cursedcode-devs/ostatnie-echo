using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wyświetla treść reklamy jako napisy (subtitle) podczas emisji na antenie.
///
/// Tempo jest NATURALNE — wyliczane z długości tekstu (prędkość czytania), a nie z długości
/// placeholderowego dźwięku. Dzięki temu długa reklama nie jest ucinana przez krótkie audio.
/// Jeśli dźwięk jest dłuższy niż naturalny czas czytania (np. prawdziwe nagranie), napisy
/// rozciągają się tak, by zgrać się z audio.
///
/// "Chumorek": fragmenty w nawiasach ( ... ) — jak drobny druczek / aside — czytane są szybciej
/// (np. dyskalimer E.V.A. „Badania sponsorowane przez...").
///
/// WSPÓŁDZIELENIE OBIEKTU Z TELEFONEM:
/// Telefon i reklamy nigdy nie grają jednocześnie, więc reklama korzysta z tego samego okna
/// dialogowego co rozmowy (PhoneCallMiniGame) — identyczna czcionka/styl, bez duplikatów UI.
/// Fallback: gdy brak telefonu w scenie, budowany jest własny pasek napisów.
/// </summary>
public class AdSubtitleDisplay : MonoBehaviour
{
    public static AdSubtitleDisplay Instance { get; private set; }

    [Header("Współdzielone okno dialogu (opcjonalne)")]
    [Tooltip("Okno dialogu telefonu, którego UI reklama pożycza. Jeśli puste, zostanie wyszukane w scenie (także nieaktywne).")]
    public PhoneCallMiniGame phoneDialog;

    [Header("Własne referencje UI (fallback — gdy brak telefonu)")]
    public GameObject subtitlePanel;
    public TextMeshProUGUI subtitleText;
    [Tooltip("Czcionka dla trybu fallback. Przy współdzieleniu z telefonem używana jest jego czcionka.")]
    public TMP_FontAsset font;

    [Header("Tempo czytania")]
    [Tooltip("Naturalna prędkość czytania (znaki/sekundę). ~15 = czytanie na głos.")]
    public float charsPerSecond = 85f;
    [Tooltip("Szybsze tempo dla fragmentów w nawiasach (drobny druczek / aside).")]
    public float fastCharsPerSecond = 100f;
    [Tooltip("Krótka pauza po każdej linii (s) — naturalny rytm.")]
    public float sentencePause = 0.10f;
    [Tooltip("Minimalny czas wyświetlania jednej linii (s).")]
    public float minLineDuration = 0.3f;
    [Tooltip("Docelowa maksymalna długość bloku napisów (znaki). Krótsze zdania pokazywane są w całości (pasek zawija tekst); dłuższe dzielone po przecinkach.")]
    public int maxCharsPerLine = 120;
    [Tooltip("Jeśli po podziale długiego zdania ogon jest krótszy niż tyle znaków, doklejamy go do poprzedniego bloku (żeby końcówka zdania nie wisiała sama).")]
    public int minTailChars = 35;

    /// <summary>Czy napisy są aktualnie wyświetlane (emisja reklamy trwa).</summary>
    public bool IsShowing => subtitleRoutine != null;

    // Rozwiązany cel wyświetlania (telefon lub fallback)
    private TextMeshProUGUI activeText;
    private GameObject activeRoot;
    private readonly List<GameObject> hiddenForAd = new List<GameObject>();
    private bool resolved;

    private Coroutine subtitleRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Znajduje istniejący wyświetlacz napisów lub tworzy nowy (lazy).</summary>
    public static AdSubtitleDisplay GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        var existing = FindFirstObjectByType<AdSubtitleDisplay>();
        if (existing != null)
            return existing;

        var go = new GameObject("AdSubtitleDisplay");
        return go.AddComponent<AdSubtitleDisplay>();
    }

    /// <summary>
    /// Pokazuje treść reklamy jako napisy w naturalnym tempie czytania.
    /// </summary>
    /// <param name="content">Pełna treść reklamy.</param>
    /// <param name="audioDuration">Długość dźwięku (s). Jeśli &gt; naturalnego czasu czytania, napisy rozciągają się do audio.</param>
    public void ShowAd(string content, float audioDuration)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            Hide();
            return;
        }

        ResolveTarget();
        if (activeText == null)
            return;

        if (subtitleRoutine != null)
            StopCoroutine(subtitleRoutine);

        ShowDialogWindow();
        subtitleRoutine = StartCoroutine(PlaySubtitles(content, audioDuration));
    }

    /// <summary>Ukrywa napisy i przywraca okno do stanu spoczynku.</summary>
    public void Hide()
    {
        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
            subtitleRoutine = null;
        }
        HideImmediate();
    }

    // ------------------------------------------------------------------

    private void ResolveTarget()
    {
        if (resolved)
            return;
        resolved = true;

        if (subtitleText != null)
        {
            activeText = subtitleText;
            activeRoot = subtitlePanel;
            return;
        }

        if (phoneDialog == null)
        {
            var phones = FindObjectsByType<PhoneCallMiniGame>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (phones != null && phones.Length > 0)
                phoneDialog = phones[0];
        }

        if (phoneDialog != null && phoneDialog.dialogText != null)
        {
            activeText = phoneDialog.dialogText;
            activeRoot = phoneDialog.miniGameCanvas != null ? phoneDialog.miniGameCanvas : phoneDialog.gameObject;
            return;
        }

        BuildRuntimeUI();
        activeText = subtitleText;
        activeRoot = subtitlePanel;
    }

    private void ShowDialogWindow()
    {
        if (activeRoot != null)
            activeRoot.SetActive(true);

        hiddenForAd.Clear();
        if (phoneDialog != null && activeText == phoneDialog.dialogText)
        {
            HideIfActive(phoneDialog.optionAButton);
            HideIfActive(phoneDialog.optionBButton);
            HideIfActive(phoneDialog.okayButton);
        }
    }

    private void HideIfActive(Button btn)
    {
        if (btn != null && btn.gameObject.activeSelf)
        {
            btn.gameObject.SetActive(false);
            hiddenForAd.Add(btn.gameObject);
        }
    }

    private void HideImmediate()
    {
        if (activeText != null)
        {
            activeText.maxVisibleCharacters = int.MaxValue;
            activeText.text = "";
        }
        if (activeRoot != null)
            activeRoot.SetActive(false);

        hiddenForAd.Clear();
    }

    private IEnumerator PlaySubtitles(string content, float audioDuration)
    {
        List<string> lines = SplitIntoLines(content);
        if (lines.Count == 0)
        {
            HideImmediate();
            subtitleRoutine = null;
            yield break;
        }

        // Naturalny czas każdej linii (prędkość czytania zależna od stylu linii).
        float[] natural = new float[lines.Count];
        float naturalTotal = 0f;
        for (int i = 0; i < lines.Count; i++)
        {
            float speed = LineSpeed(lines[i]);
            float t = Mathf.Max(minLineDuration, lines[i].Length / speed) + sentencePause;
            natural[i] = t;
            naturalTotal += t;
        }

        // Jeśli dźwięk jest dłuższy niż naturalny czas czytania (prawdziwe nagranie) —
        // rozciągnij napisy, by zgrać się z audio. W przeciwnym razie zostaw naturalne tempo
        // (tekst nie zostanie ucięty przez krótki placeholder).
        float scale = 1f;
        if (audioDuration > naturalTotal && naturalTotal > 0f)
            scale = audioDuration / naturalTotal;

        for (int i = 0; i < lines.Count; i++)
        {
            yield return StartCoroutine(TypeLine(lines[i], natural[i] * scale));
        }

        HideImmediate();
        subtitleRoutine = null;
    }

    /// <summary>Prędkość czytania danej linii — szybsza dla fragmentów w nawiasach (drobny druczek).</summary>
    private float LineSpeed(string line)
    {
        string t = line.TrimStart();
        if (t.StartsWith("(") || t.StartsWith("（"))
            return fastCharsPerSecond;
        return charsPerSecond;
    }

    private IEnumerator TypeLine(string line, float duration)
    {
        if (activeText == null)
            yield break;

        activeText.text = line;
        activeText.maxVisibleCharacters = 0;
        activeText.ForceMeshUpdate();
        int total = activeText.textInfo.characterCount;

        // Krótki "ogon" — gotowa linia chwilę zostaje na ekranie.
        float hold = Mathf.Min(sentencePause, duration * 0.3f);
        float revealTime = Mathf.Max(0.01f, duration - hold);
        float perChar = total > 0 ? revealTime / total : 0f;

        int counter = 0;
        while (counter < total)
        {
            counter++;
            activeText.maxVisibleCharacters = counter;
            if (perChar > 0f)
                yield return new WaitForSecondsRealtime(perChar);
            else
                yield return null;
        }
        activeText.maxVisibleCharacters = total;

        if (hold > 0f)
            yield return new WaitForSecondsRealtime(hold);
    }

    /// <summary>
    /// Dzieli treść na pojedyncze linie napisów: najpierw po zdaniach (. ! ?),
    /// a zbyt długie zdania dodatkowo po słowach do maxCharsPerLine.
    /// </summary>
    private List<string> SplitIntoLines(string content)
    {
        var result = new List<string>();
        content = content.Replace("\r", " ").Replace("\n", " ").Trim();
        if (content.Length == 0)
            return result;

        var sentences = new List<string>();
        int start = 0;
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (c == '.' || c == '!' || c == '?')
            {
                while (i + 1 < content.Length && (content[i + 1] == '.' || content[i + 1] == '!' || content[i + 1] == '?'))
                    i++;
                string s = content.Substring(start, i - start + 1).Trim();
                if (s.Length > 0)
                    sentences.Add(s);
                start = i + 1;
            }
        }
        if (start < content.Length)
        {
            string tail = content.Substring(start).Trim();
            if (tail.Length > 0)
                sentences.Add(tail);
        }

        foreach (var sentence in sentences)
        {
            // Całe zdanie = jeden blok (pasek zawija tekst na kilka linii wizualnych).
            if (sentence.Length <= maxCharsPerLine)
            {
                result.Add(sentence);
                continue;
            }

            // Tylko bardzo długie zdania dzielimy — preferując podział po przecinkach.
            result.AddRange(SplitLongSentence(sentence));
        }

        return result;
    }

    /// <summary>
    /// Dzieli zbyt długie zdanie na bloki, łamiąc najchętniej po przecinku/średniku.
    /// Zbyt krótki "ogon" (końcówka zdania) jest doklejany do poprzedniego bloku,
    /// żeby nie wisiał sam w osobnym bloku.
    /// </summary>
    private List<string> SplitLongSentence(string sentence)
    {
        var chunks = new List<string>();
        string[] words = sentence.Split(' ');
        string current = "";

        foreach (var w in words)
        {
            string candidate = current.Length == 0 ? w : current + " " + w;

            if (candidate.Length > maxCharsPerLine && current.Length > 0)
            {
                chunks.Add(current);
                current = w;
            }
            else
            {
                current = candidate;
                // Naturalny podział: gdy blok jest już sensownej długości i kończymy klauzulę.
                if (current.Length >= maxCharsPerLine * 0.6f &&
                    (w.EndsWith(",") || w.EndsWith(";") || w.EndsWith(":") || w.EndsWith("–") || w.EndsWith("-")))
                {
                    chunks.Add(current);
                    current = "";
                }
            }
        }
        if (current.Length > 0)
            chunks.Add(current);

        // Doklej zbyt krótki ogon do poprzedniego bloku.
        if (chunks.Count >= 2 && chunks[chunks.Count - 1].Length < minTailChars)
        {
            chunks[chunks.Count - 2] += " " + chunks[chunks.Count - 1];
            chunks.RemoveAt(chunks.Count - 1);
        }

        return chunks;
    }

    // ------------------------------------------------------------------
    // Fallback: prosty pasek napisów u góry (gdy w scenie nie ma okna telefonu).

    private void BuildRuntimeUI()
    {
        var canvasGo = new GameObject("AdSubtitleCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        var barGo = new GameObject("DialogBackground");
        barGo.transform.SetParent(canvasGo.transform, false);
        var barRect = barGo.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.sizeDelta = new Vector2(0f, 160f);
        barRect.anchoredPosition = Vector2.zero;
        var barImg = barGo.AddComponent<Image>();
        barImg.color = new Color(0f, 0f, 0f, 0.92f);
        barImg.raycastTarget = false;

        var textGo = new GameObject("SubtitleText");
        textGo.transform.SetParent(barGo.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(40f, 10f);
        textRect.offsetMax = new Vector2(-40f, -10f);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (font != null)
            tmp.font = font;
        tmp.text = "";
        tmp.color = new Color(1f, 0.458f, 0f, 1f);
        tmp.fontStyle = FontStyles.Italic;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 28f;
        tmp.fontSizeMax = 64f;
        tmp.raycastTarget = false;

        subtitleText = tmp;
        subtitlePanel = canvasGo;
    }
}
