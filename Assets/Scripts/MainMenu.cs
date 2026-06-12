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

    private void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);

        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(OpenHowToPlay);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("MergeScene");
    }

    public void ResumeGame()
    {
        // Zostanie zaimplementowane później (mechanizm pauzy)
        Debug.Log("Wznów grę (Resume)");
    }

    public void OpenHowToPlay()
    {
        // Zostanie zaimplementowane później
        Debug.Log("Jak grać (How to Play)");
    }

    public void OpenSettings()
    {
        // Zostanie zaimplementowane później
        Debug.Log("Ustawienia (Settings)");
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
