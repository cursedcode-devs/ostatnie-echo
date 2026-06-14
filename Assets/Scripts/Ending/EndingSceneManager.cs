using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// EndingSceneManager — narracyjne zakończenie gry, wbudowane w HepiScene.
/// ================================================================================
/// Komponent siedzi na ZAWSZE AKTYWNYM obiekcie "EndingManager" w HepiScene,
/// a steruje ukrytym Canvasem (endingRoot). DayEndHandler woła Play(), gdy gracz
/// dotrwa do końca gry (endGameCause == 0). Styl 1:1 jak telegazeta (NewspaperScene):
///   - Canvas (ScreenSpaceOverlay) z elementami przypisanymi w inspektorze,
///   - Time.timeScale = 0 na czas pokazu,
///   - przyciski w kolorach telegazety (Jersey10).
///
/// Przebieg:
///   1. Pokazuje po kolei 3 "telegazety" (PNG) — po jednej na oś zakończenia
///      (Prowadzący / Słuchacz / Rząd). Wariant a/b z EndingData (ustawia DayEndHandler).
///      Gracz przewija je przyciskiem "DALEJ" (jak gazetę).
///   2. Czarny ekran z liczbą słuchaczy rosnącą coraz szybciej aż do zatrzymania.
///   3. Napis "Usłyszało cię:" nad liczbą + przycisk wyjścia.
/// </summary>
public class EndingSceneManager : MonoBehaviour
{
    public GameManager GameManager;
    [Header("Telegazety — Prowadzący (Host)")]
    [Tooltip("a) Widownia pomaga prowadzącemu uciec do bunkra (przeżywa).")]
    public Sprite hostSurvives;
    [Tooltip("b) Nikt nie poczekał ani nie pomógł (ginie).")]
    public Sprite hostDies;

    [Header("Telegazety — Słuchacz (Listener)")]
    [Tooltip("a) Subkultury zakopują topór, organizują się i pomagają ludziom.")]
    public Sprite listenersUnite;
    [Tooltip("b) Starcia między subkulturami narastają, dochodzi do walk.")]
    public Sprite listenersFight;

    [Header("Telegazety — Rząd (Government)")]
    [Tooltip("a) Antysystemowcy nadają sygnał ewakuacyjny, mniej ofiar.")]
    public Sprite governmentSignal;
    [Tooltip("b) Prokomunistyczne marionetki uciszają sygnały o konflikcie.")]
    public Sprite governmentSilence;

    [Header("Elementy UI (przypisz w scenie)")]
    [Tooltip("Image, na którym wyświetlają się telegazety (z preserveAspect).")]
    public Image pageImage;
    [Tooltip("Czarny panel zakrywający telegazety na czas ekranu licznika.")]
    public GameObject blackPanel;
    [Tooltip("Duża liczba słuchaczy na środku ekranu licznika.")]
    public TextMeshProUGUI countText;
    [Tooltip("Napis 'Usłyszało cię:' nad liczbą.")]
    public TextMeshProUGUI countLabel;
    [Tooltip("Werdykt pod liczbą: 'Gratulacje.' albo 'Nie osiągnąłeś rozgłosu...'.")]
    public TextMeshProUGUI verdictText;
    [Tooltip("Przycisk przewijania telegazet ('DALEJ').")]
    public Button nextButton;
    [Tooltip("Przycisk wyjścia (po zatrzymaniu licznika).")]
    public Button exitButton;
    [Tooltip("Korzeń UI zakończenia (Canvas). Wyłączany do czasu Play(). " +
             "Ten skrypt musi siedzieć na ZAWSZE AKTYWNYM obiekcie (nie na tym Canvasie), " +
             "inaczej korutyny się nie uruchomią.")]
    public GameObject endingRoot;

    [Header("Animacja (czas nieskalowany)")]
    [Tooltip("Czas fade slajdów-telegazet (pojawianie/znikanie obrazków).")]
    public float fadeDuration = 0.5f;
    [Tooltip("Czas fade napisów końcowych (Usłyszało cię / werdykt / przycisk).")]
    public float textFadeDuration = 0.5f;
    [Tooltip("Minimalny czas zliczania (małe liczby) — sekundy.")]
    public float countMinDuration = 2f;
    [Tooltip("Maksymalny czas zliczania (duże liczby) — sekundy.")]
    public float countMaxDuration = 9f;
    [Tooltip("Ile sekund dodaje każdy rząd wielkości liczby. Czas = min + log10(liczba) * to, ograniczone do max.")]
    public float countSecondsPerDigit = 1.2f;
    [Tooltip("Stromość WYKŁADNICZEJ krzywej licznika. Im więcej, tym dłuższy wolny start i gwałtowniejszy koniec (np. 3-8).")]
    public float countAcceleration = 5f;
    [Tooltip("Pauza między kolejnymi napisami wchodzącymi z fade-in.")]
    public float sequentialDelay = 0.25f;

    [Header("Teksty")]
    public string listenersLabel = "Usłyszało cię:";
    [Tooltip("Dopisek po liczbie słuchaczy, np. \"osób\".")]
    public string countSuffix = "osób";
    [Tooltip("Napis na przycisku przewijania. Bez znaków spoza czcionki (np. ▶ nie istnieje w Jersey10).")]
    public string nextButtonLabel = "DALEJ";
    public string exitButtonLabel = "WYJŚCIE";

    [Header("Werdykt — próg rozgłosu")]
    [Tooltip("Minimalna liczba słuchaczy, by zobaczyć pełne zakończenie (wybuch + telegazety).")]
    public int fameThreshold = 100;
    public string successVerdict = "Gratulacje.";
    public string failVerdict = "Nie osiągnąłeś rozgłosu. Spróbuj jeszcze raz.";

    [Header("Intro — czarny ekran + dźwięk (konflikt nuklearny)")]
    [Tooltip("Dźwięk odtwarzany na czarnym ekranie przed telegazetami (syrena/wybuch).")]
    public AudioClip introSound;
    [Tooltip("Ile trwa czarny ekran przed telegazetami. 0 = długość dźwięku.")]
    public float introBlackDuration = 0f;
    [Tooltip("Dodatkowa przerwa (czarny ekran) PO dźwięku wybuchu, przed telegazetami — sekundy.")]
    public float postSoundDelay = 3f;

    [Header("Dźwięki otoczenia")]
    [Tooltip("Wyciszyć dźwięki otoczenia/gry (FMOD + Unity) na czas zakończenia. Dźwięk wybuchu gra dalej.")]
    public bool muteEnvironmentAudio = true;
    [Tooltip("Ścieżka FMOD bus do wyciszenia (domyślnie master).")]
    public string audioBusPath = "bus:/";

    private float prevListenerVolume = 1f;
    private bool audioMuted = false;

    private bool advanceRequested = false;
    private AudioSource audioSource;

    void Start()
    {
        // Wbudowane w HepiScene: nie uruchamiamy się sami — czekamy aż
        // DayEndHandler wywoła Play() po ukończeniu gry. UI trzymamy ukryte.
        if (endingRoot != null) endingRoot.SetActive(false);
    }

    /// <summary>
    /// Odpala sekwencję zakończenia. Wołane z DayEndHandler, gdy gracz dotrwał
    /// do końca gry (endGameCause == 0). Wariant a/b czytany z EndingData.
    /// </summary>
    public void Play()
    {
        if (endingRoot != null) endingRoot.SetActive(true);
        EnsureEventSystem();
        Time.timeScale = 0f;
        SetEnvironmentAudioMuted(true);

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => advanceRequested = true);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(Quit);
        }

        // Ustaw etykiety w kodzie — gwarantuje brak "tofu" (np. ▶ poza czcionką Jersey10).
        SetButtonLabel(nextButton, nextButtonLabel);
        SetButtonLabel(exitButton, exitButtonLabel);

        Sprite[] pages =
        {
            EndingData.HostSurvives     ? hostSurvives     : hostDies,
            EndingData.ListenersUnite   ? listenersUnite   : listenersFight,
            EndingData.GovernmentSignal ? governmentSignal : governmentSilence,
        };

        StartCoroutine(Run(pages, EndingData.FinalListeners));
    }

    IEnumerator Run(Sprite[] pages, int finalListeners)
    {
        // Na start wszystko ukryte, czarny ekran na wierzchu.
        if (pageImage != null) pageImage.gameObject.SetActive(false);
        if (blackPanel != null) blackPanel.SetActive(true);
        if (countText != null) countText.gameObject.SetActive(false);
        if (countLabel != null) countLabel.gameObject.SetActive(false);
        if (verdictText != null) verdictText.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);

        // Próg rozgłosu decyduje o wariancie zakończenia.
        bool success = finalListeners >= fameThreshold;

        //cursed code
        if (GameManager.radioStation.currentListeners.hipHop <= 0 || GameManager.radioStation.currentListeners.disco <= 0 || GameManager.radioStation.currentListeners.pop <= 0 || GameManager.radioStation.currentListeners.rock <= 0)
        {
            success = false;
        }

        if (success)
        {
            // 0) Czarny ekran + dźwięk wybuchu — następuje konflikt nuklearny.
            PlayIntroSound();
            float introDur = introBlackDuration > 0f
                ? introBlackDuration
                : (introSound != null ? introSound.length : 2f);
            float tb = 0f;
            while (tb < introDur)
            {
                tb += Time.unscaledDeltaTime;
                yield return null;
            }

            // Dodatkowa przerwa po dźwięku wybuchu (czarny ekran) przed telegazetami.
            float tp = 0f;
            while (tp < postSoundDelay)
            {
                tp += Time.unscaledDeltaTime;
                yield return null;
            }

            // 1) Telegazety — gracz przewija je przyciskiem "DALEJ"
            if (blackPanel != null) blackPanel.SetActive(false);
            if (pageImage != null) pageImage.gameObject.SetActive(true);
            foreach (var page in pages)
            {
                if (page == null || pageImage == null) continue;

                pageImage.sprite = page;
                yield return Fade(pageImage, 0f, 1f, fadeDuration);

                if (nextButton != null) nextButton.gameObject.SetActive(true);
                yield return WaitForAdvance();
                if (nextButton != null) nextButton.gameObject.SetActive(false);

                yield return Fade(pageImage, 1f, 0f, fadeDuration);
            }

            if (pageImage != null) pageImage.gameObject.SetActive(false);
            if (blackPanel != null) blackPanel.SetActive(true);
        }
        // Wariant porażki (< próg): bez dźwięku i bez telegazet — od razu licznik.

        // 2) Czarny ekran z licznikiem słuchaczy (rośnie coraz szybciej) — ZAWSZE.
        if (countText != null)
        {
            countText.gameObject.SetActive(true);
            countText.text = FormatCount(0);
        }

        // Czas zliczania zależny od wielkości liczby (więcej słuchaczy -> dłużej).
        float countDur = Mathf.Clamp(
            countMinDuration + Mathf.Log10(Mathf.Max(1, finalListeners)) * countSecondsPerDigit,
            countMinDuration, countMaxDuration);

        float t = 0f;
        float k = Mathf.Max(0.0001f, countAcceleration); // stromość krzywej wykładniczej
        float denom = Mathf.Exp(k) - 1f;
        while (t < countDur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / countDur);
            // Wykładniczo: prędkość rośnie e^(k*p) — bardzo wolny start, gwałtowny koniec.
            float eased = (Mathf.Exp(k * p) - 1f) / denom;
            int shown = Mathf.RoundToInt(Mathf.Lerp(0f, finalListeners, eased));
            if (countText != null) countText.text = FormatCount(shown);
            yield return null;
        }
        if (countText != null) countText.text = FormatCount(finalListeners);

        // 3) Napisy wchodzą PO KOLEI z fade-in: "Usłyszało cię:" -> werdykt -> "Wyjście".
        if (countLabel != null)
        {
            countLabel.text = listenersLabel;
            yield return FadeInGraphic(countLabel, textFadeDuration);
            yield return new WaitForSecondsRealtime(sequentialDelay);
        }
        if (verdictText != null)
        {
            verdictText.text = success ? successVerdict : failVerdict;
            yield return FadeInGraphic(verdictText, textFadeDuration);
            yield return new WaitForSecondsRealtime(sequentialDelay);
        }
        if (exitButton != null)
            yield return FadeInCanvasGroup(exitButton.gameObject, textFadeDuration);
    }

    // Fade-in dla tekstu (TMP) — alpha 0 -> 1 na czasie nieskalowanym.
    IEnumerator FadeInGraphic(Graphic g, float dur)
    {
        if (g == null) yield break;
        var col = g.color; col.a = 0f; g.color = col;
        g.gameObject.SetActive(true);
        if (dur <= 0f) { col.a = 1f; g.color = col; yield break; }
        float e = 0f;
        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            col.a = Mathf.Clamp01(e / dur); g.color = col;
            yield return null;
        }
        col.a = 1f; g.color = col;
    }

    // Fade-in dla przycisku (przez CanvasGroup) — alpha 0 -> 1.
    IEnumerator FadeInCanvasGroup(GameObject go, float dur)
    {
        if (go == null) yield break;
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        go.SetActive(true);
        if (dur <= 0f) { cg.alpha = 1f; yield break; }
        float e = 0f;
        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(e / dur);
            yield return null;
        }
        cg.alpha = 1f;
    }

    string FormatCount(int n)
    {
        return string.IsNullOrEmpty(countSuffix) ? n.ToString() : $"{n} {countSuffix}";
    }

    void SetButtonLabel(Button btn, string text)
    {
        if (btn == null) return;
        var t = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (t != null) t.text = text;
    }

    void PlayIntroSound()
    {
        if (introSound == null) return;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.ignoreListenerPause = true;  // gra mimo Time.timeScale = 0 / pauzy
        audioSource.ignoreListenerVolume = true; // gra mimo wyciszenia AudioListener (wyciszamy otoczenie)
        audioSource.PlayOneShot(introSound);
    }

    /// <summary>Wycisza/odcisza dźwięki otoczenia: FMOD (master bus) + Unity (AudioListener).</summary>
    void SetEnvironmentAudioMuted(bool muted)
    {
        if (!muteEnvironmentAudio) return;

        // FMOD — ambient (syreny/auta) i efekty gry. Mute busa + zatrzymanie eventów.
        try
        {
            var bus = FMODUnity.RuntimeManager.GetBus(audioBusPath);
            if (bus.isValid())
            {
                bus.setMute(muted);
                if (muted) bus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Ending] Nie udało się wyciszyć FMOD bus '" + audioBusPath + "': " + e.Message);
        }

        // Pewniak: wycisz główny channel group rdzenia FMOD (całe wyjście, niezależnie od busów).
        try
        {
            FMOD.ChannelGroup master;
            if (FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out master) == FMOD.RESULT.OK)
                master.setMute(muted);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Ending] Nie udało się wyciszyć FMOD core master: " + e.Message);
        }

        // Unity — np. muzyka radia (AudioQueueManager). Wybuch ma ignoreListenerVolume.
        if (muted)
        {
            if (!audioMuted) prevListenerVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = prevListenerVolume;
        }
        audioMuted = muted;
    }

    IEnumerator WaitForAdvance()
    {
        advanceRequested = false;
        while (!advanceRequested) yield return null;
        advanceRequested = false;
    }

    IEnumerator Fade(Graphic g, float from, float to, float dur)
    {
        if (g == null) yield break;
        if (dur <= 0f)
        {
            var ic = g.color; ic.a = to; g.color = ic;
            yield break;
        }

        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            var cc = g.color; cc.a = Mathf.Lerp(from, to, t / dur); g.color = cc;
            yield return null;
        }
        var fc = g.color; fc.a = to; g.color = fc;
    }

    void Quit()
    {
        Time.timeScale = 1f;
        SetEnvironmentAudioMuted(false);
        //if (endingRoot != null) endingRoot.SetActive(false);
        // Załaduj scenę MainMenu od nowa (tryb Single)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Gwarantuje EventSystem (przyciski UI go wymagają).</summary>
    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
    }
}
