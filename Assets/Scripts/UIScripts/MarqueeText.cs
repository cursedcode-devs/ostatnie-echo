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

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = (RectTransform)transform.parent;
        tmpText = GetComponent<TMP_Text>();
    }

    public void ResetPosition()
    {
        Canvas.ForceUpdateCanvases();
        tmpText.ForceMeshUpdate();

        textWidth = tmpText.preferredWidth;
        parentWidth = parentRect.rect.width;

        startX = parentWidth / 2f + textWidth / 2f + 20f;

        rectTransform.anchoredPosition =
            new Vector2(startX, rectTransform.anchoredPosition.y);
    }

    void Update()
    {
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x - speed * Time.deltaTime,
            rectTransform.anchoredPosition.y
        );

        if (rectTransform.anchoredPosition.x < -(parentWidth / 2f + textWidth))
        {
            rectTransform.anchoredPosition =
                new Vector2(startX, rectTransform.anchoredPosition.y);
        }
    }
}