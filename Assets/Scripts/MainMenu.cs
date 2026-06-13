using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject titleImage;

    [Header("Ustawienia Audio")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button backFromSettingsButton;
    [SerializeField] private string masterBusPath = "bus:/";
    private FMOD.Studio.Bus masterBus;

    [Header("Instrukcje (Jak grać)")]
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private TMPro.TextMeshProUGUI howToPlayText;
    [SerializeField] private Button howToPlayNextBtn;
    [SerializeField] private Button howToPlayPrevBtn;
    [SerializeField] private Button backFromHowToPlayBtn;
    [TextArea(5, 15)]
    [SerializeField] private string[] howToPlaySlides;
    private int currentSlideIndex = 0;

    private CanvasGroup canvasGroup;
    private UnityEngine.Video.VideoPlayer videoPlayer;
    private bool isFading = false;

    private void Start()
    {
        bool isPaused = SceneManager.sceneCount > 1;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && isPaused)
        {
            // Zapewnia, że Canvas MainMenu wyświetla się całkowicie NA WIERZCHU, zasłaniając UI z MergeScene
            canvas.sortingOrder = 100;
        }

        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(isPaused);
            resumeButton.onClick.AddListener(ResumeGame);
        }
            
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);

        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(OpenHowToPlay);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Domyślnie ukrywamy ustawienia
        }

        if (backFromSettingsButton != null)
        {
            backFromSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (howToPlayNextBtn != null)
            howToPlayNextBtn.onClick.AddListener(NextSlide);

        if (howToPlayPrevBtn != null)
            howToPlayPrevBtn.onClick.AddListener(PrevSlide);

        if (backFromHowToPlayBtn != null)
            backFromHowToPlayBtn.onClick.AddListener(CloseHowToPlay);

        if (volumeSlider != null)
        {
            masterBus = FMODUnity.RuntimeManager.GetBus(masterBusPath);
            float currentVolume;
            masterBus.getVolume(out currentVolume);
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (isPaused)
        {
            foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
            {
                if (obj.name == "Main Camera")
                {
                    videoPlayer = obj.GetComponent<UnityEngine.Video.VideoPlayer>();
                    Camera cam = obj.GetComponent<Camera>();
                    if (cam != null)
                    {
                        cam.enabled = false; // Wyłączamy kamerę, by nie renderowała się pod UI
                    }
                }
            }

            if (videoPlayer != null)
            {
                // Tworzymy dynamicznie teksturę pod wideo, aby wyświetlić je wewnątrz UI Canvasu na samym wierzchu
                RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
                videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = rt;

                GameObject rawImageObj = new GameObject("VideoRawImage");
                rawImageObj.transform.SetParent(transform, false);
                rawImageObj.transform.SetAsFirstSibling(); // Tło wideo pod przyciskami
                
                UnityEngine.UI.RawImage videoRawImage = rawImageObj.AddComponent<UnityEngine.UI.RawImage>();
                videoRawImage.texture = rt;
                
                RectTransform rect = videoRawImage.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            StartCoroutine(FadeIn());
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null && videoPlayer.targetTexture != null)
        {
            videoPlayer.targetTexture.Release();
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        isFading = true;
        float duration = 0.3f;
        float elapsed = 0f;
        
        canvasGroup.alpha = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / duration;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        isFading = false;
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MergeScene");
    }

    public void ResumeGame()
    {
        if (isFading) return;
        
        if (SceneManager.sceneCount == 1)
        {
            Debug.Log("Wznów grę (Resume)");
            return;
        }
        
        StartCoroutine(FadeOutAndUnload());
    }

    private System.Collections.IEnumerator FadeOutAndUnload()
    {
        isFading = true;
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (canvasGroup != null) canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }
        
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.SetUnpaused();
        }
        else
        {
            Time.timeScale = 1f;
        }
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void OpenHowToPlay()
    {
        ToggleMainButtons(false);
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            currentSlideIndex = 0;
            UpdateSlideText();
        }
    }

    public void CloseHowToPlay()
    {
        ToggleMainButtons(true);
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    public void NextSlide()
    {
        if (howToPlaySlides == null || howToPlaySlides.Length == 0) return;
        if (currentSlideIndex < howToPlaySlides.Length - 1)
        {
            currentSlideIndex++;
            UpdateSlideText();
        }
    }

    public void PrevSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            UpdateSlideText();
        }
    }

    private void UpdateSlideText()
    {
        if (howToPlayText != null && howToPlaySlides != null && howToPlaySlides.Length > 0)
        {
            howToPlayText.text = howToPlaySlides[currentSlideIndex];
        }

        if (howToPlayPrevBtn != null)
            howToPlayPrevBtn.interactable = (currentSlideIndex > 0);
            
        if (howToPlayNextBtn != null && howToPlaySlides != null)
            howToPlayNextBtn.interactable = (currentSlideIndex < howToPlaySlides.Length - 1);
    }

    public void OpenSettings()
    {
        ToggleMainButtons(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        ToggleMainButtons(true);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void SetVolume(float volume)
    {
        masterBus.setVolume(volume);
    }

    private void ToggleMainButtons(bool active)
    {
        bool isPaused = SceneManager.sceneCount > 1;
        if (resumeButton != null) resumeButton.gameObject.SetActive(active && isPaused);
        if (newGameButton != null) newGameButton.gameObject.SetActive(active);
        if (howToPlayButton != null) howToPlayButton.gameObject.SetActive(active);
        if (settingsButton != null) settingsButton.gameObject.SetActive(active);
        if (exitButton != null) exitButton.gameObject.SetActive(active);
        if (titleImage != null) titleImage.SetActive(active);
    }

    public void ExitGame()
    {
        Debug.Log("Wyjście z gry!");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
