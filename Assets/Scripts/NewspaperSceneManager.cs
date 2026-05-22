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

    void BindDataToUI()
    {
        if (titleText != null)
            titleText.text = $"DZIEŃ {(DaySummaryData.Day > 0 ? DaySummaryData.Day - 1 : 1)} — GAZETA";
    }

    public void OnContinueClicked()
    {
        Time.timeScale = 1f;

        DaySummaryData.OnSummaryClosed?.Invoke();
        DaySummaryData.OnSummaryClosed = null;

        SceneManager.UnloadSceneAsync("NewspaperScene");
    }
}
