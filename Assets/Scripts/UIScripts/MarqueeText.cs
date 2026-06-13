using TMPro;
using UnityEngine;

public class MarqueeText : MonoBehaviour
{
    [SerializeField] private float speed = 60f;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private TMP_Text tmpText;

    private float textWidth;
    private float parentWidth;
    private float startX;
    private Vector2 lastScreenSize;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = (RectTransform)transform.parent;
        tmpText = GetComponent<TMP_Text>();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    public void ResetPosition(bool keepXPosition = false)
    {
        Canvas.ForceUpdateCanvases();
        tmpText.ForceMeshUpdate();
        textWidth = tmpText.preferredWidth;
        float textHeight = tmpText.preferredHeight;
        float verticalPadding = 20f;
        parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight + verticalPadding);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
        parentWidth = parentRect.rect.width;
        startX = parentRect.rect.xMax + (textWidth * rectTransform.pivot.x);
        float startY = parentRect.rect.center.y + (textHeight * (rectTransform.pivot.y - 0.5f));
        // ZMIANA: u¿ywamy localPosition zamiast anchoredPosition. 
        // Zachowujemy te¿ stary wymiar Z obiektu.
        float finalX = keepXPosition ? rectTransform.localPosition.x : startX;
        rectTransform.localPosition = new Vector3(finalX, startY, rectTransform.localPosition.z);

    }

    void Update()
    {
        // Sprawdzanie zmiany rozdzielczoœci okna z poprzedniego etapu
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            ResetPosition(true);
        }
        // ZMIANA: przesuwanie w lewo na bazie localPosition
        rectTransform.localPosition = new Vector3(
            rectTransform.localPosition.x - speed * Time.deltaTime,
            rectTransform.localPosition.y,
            rectTransform.localPosition.z
        );
        // ZMIANA: liczymy praw¹ krawêdŸ bazuj¹c w 100% na localPosition
        float textRightEdge = rectTransform.localPosition.x + (textWidth * (1f - rectTransform.pivot.x));
        // ZMIANA: teleport u¿ywaj¹cy localPosition
        if (textRightEdge < parentRect.rect.xMin)
        {
            rectTransform.localPosition = new Vector3(startX, rectTransform.localPosition.y, rectTransform.localPosition.z);
        }
    }
}