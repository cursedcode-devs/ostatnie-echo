using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Po kliknięciu "Dalej" wyładowuje scenę i wywołuje DaySummaryData.OnNewspaperClosed,
/// co uruchamia kolejny dzień.
/// </summary>
public class NewspaperSceneManager : MonoBehaviour
{
    [Header("Panel gazety")]
    public GameObject newspaperPanel;

    [Header("Przycisk zamknięcia")]
    public Button continueButton;

    [Header("Opcjonalne pola tekstowe")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("DebugEventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("[DEBUG] Utworzono EventSystem (dla testów sceny gazety).");
        }
#endif

        Time.timeScale = 0f;

        if (newspaperPanel != null)
            newspaperPanel.SetActive(true);

        // tresc z DaySummaryData
        BindDataToUI();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    [Header("Obraz Gazety")]
    public RawImage newspaperImage;

    void BindDataToUI()
    {
        int dayNumber = DaySummaryData.Day > 0 ? DaySummaryData.Day - 1 : 1;
        
        if (titleText != null)
            titleText.text = $"DZIEŃ {dayNumber} — GAZETA";

        if (newspaperImage != null)
        {
            Texture2D dailyTexture = Resources.Load<Texture2D>($"TelePrasa{dayNumber}");

            if (dailyTexture != null)
            {
                newspaperImage.texture = dailyTexture;
            }
            else
            {
                Texture2D placeholderTexture = Resources.Load<Texture2D>("TelePrasaPlaceholder");
                if (placeholderTexture != null)
                    newspaperImage.texture = placeholderTexture;
                Debug.LogWarning($"[Newspaper] Nie znaleziono obrazka 'TelePrasa{dayNumber}' w folderze Resources! Upewnij się, że grafiki są w Assets/Textures/Resources/");
            }
        }
    }

    public void OnContinueClicked()
    {
        Time.timeScale = 1f;

        DaySummaryData.OnSummaryClosed?.Invoke();
        DaySummaryData.OnSummaryClosed = null;

        SceneManager.UnloadSceneAsync("NewspaperScene");
    }
}
