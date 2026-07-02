using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float distance = 20f;
    public float speed = 2f;

    private Vector2 startPos;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    private void Update()
    {
        rect.anchoredPosition = startPos +
            Vector2.up * Mathf.Sin(Time.unscaledTime * speed) * distance;
    }
}