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
        StartCoroutine(UpdateSlideRoutine());
    }
    private IEnumerator UpdateSlideRoutine()
    {
        TutorialSlide slide = slides[currentIndex];
        if (currentObjects != null)
        {
            foreach (var element in currentObjects)
                {
                    
                    element.SetActive(false);
                }
        }
        howToPlayText.gameObject.SetActive(false);
        if (slides == null || slides.Length == 0)
            yield break;

        

        if (slide.cameraTarget != null && zoomHandler != null)
        {
            yield return StartCoroutine(zoomHandler.ChangeZoomCoroutine(slide.cameraTarget));
        }

        if (howToPlayText != null)
            howToPlayText.text = slide.text;
        howToPlayText.gameObject.SetActive(true);
        if (slide.slideLayout != null && slideRoot != null)
        {
            RectTransform layout = slide.slideLayout;

            slideRoot.anchorMin = layout.anchorMin;
            slideRoot.anchorMax = layout.anchorMax;
            slideRoot.pivot = layout.pivot;
            slideRoot.anchoredPosition = layout.anchoredPosition;
            slideRoot.sizeDelta = layout.sizeDelta;
        }

        // przyciski
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
}