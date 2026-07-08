using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class Tutorial : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private TMPro.TextMeshProUGUI howToPlayText;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button prevBtn;
    [SerializeField] private Button closeBtn;

    [Header("Slides")]
    [SerializeField] private TutorialSlide[] slides;
    [SerializeField] private RectTransform slideRoot;

    [Header("Camera")]
    [SerializeField] private ZoomHandler zoomHandler;
    private GameObject[] currentObjects;
    public Transform mainCamera;
    private int currentIndex = 0;
    [SerializeField] private CanvasGroup canvasGroup;
    private Coroutine slideRoutine;
    private void Start()
    {
        if (nextBtn != null)
            nextBtn.onClick.AddListener(NextSlide);

        if (prevBtn != null)
            prevBtn.onClick.AddListener(PreviousSlide);

        if (closeBtn != null)
            closeBtn.onClick.AddListener(CloseTutorial);

        UpdateSlide();
    }

    public void OpenTutorial()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);

        currentIndex = 0;
        UpdateSlide();
    }

    public void CloseTutorial()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
        if (currentObjects != null)
        {
            foreach (var element in currentObjects)
                {
                    
                    element.SetActive(false);
                }
        }
        zoomHandler.ZoomOut();
        
    }

    public void NextSlide()
    {
        if (slides == null || slides.Length == 0) return;

        if (currentIndex < slides.Length - 1)
        {
            currentIndex++;
            UpdateSlide();
        }
    }

    public void PreviousSlide()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateSlide();
        }
    }

    public void UpdateSlide()
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(UpdateSlideRoutine());
    }
    private IEnumerator UpdateSlideRoutine()
    {
        if (slides == null || slides.Length == 0)
            yield break;

        TutorialSlide slide = slides[currentIndex];
        
        // 1. Płynne ukrycie całego panelu (tła z tekstem) ZANIM cokolwiek zmienimy
        yield return Fade(0f, 0.2f);

        if (currentObjects != null)
        {
            foreach (var element in currentObjects)
            {
                if (element != null) element.SetActive(false);
            }
        }
        
        howToPlayText.gameObject.SetActive(false);

        // 2. Ruch kamery, podczas gdy UI jest ukryte (alpha = 0)
        if (slide.cameraTarget != null && zoomHandler != null)
        {
            yield return StartCoroutine(zoomHandler.ChangeZoomCoroutine(slide.cameraTarget));
        }

        // 3. Zmiana layoutu i przygotowanie nowego tekstu
        slideRoot.anchoredPosition = slide.slideLayout.anchoredPosition;

        howToPlayText.text = slide.text;
        howToPlayText.gameObject.SetActive(true);
        
        // Opcjonalnie: można wymusić przebudowanie layoutu tutaj, ale aktywacja tekstu powyżej i tak powinna to zrobić

        // 4. Płynne pojawienie się gotowego, nowego panelu
        yield return Fade(1f, 0.2f);

        if (prevBtn != null)
            prevBtn.interactable = currentIndex > 0;

        if (nextBtn != null)
            nextBtn.interactable = currentIndex < slides.Length - 1;

        currentObjects = slide.elementsToShow;

        yield return new WaitForSeconds(1f);

        if (currentObjects != null)
        {
            foreach (var element in currentObjects)
            {
                if (element != null)
                    element.SetActive(true);
            }
        }
    }
    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float start = canvasGroup.alpha;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}