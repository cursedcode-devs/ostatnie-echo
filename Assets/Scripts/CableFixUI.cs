using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CableFixUI
/// ===========
/// Receives state from CableFixMiniGame and updates the UI.
/// Built and wired by CableFixSceneBuilder.
///
/// Coordinate system: all positions are fractions 0-1 of PlayAreaHeight,
/// converted to RectTransform anchoredPosition here.
/// </summary>
public class CableFixUI : MonoBehaviour
{
    // Set by CableFixSceneBuilder
    [HideInInspector] public RectTransform barRect;
    [HideInInspector] public RectTransform zoneRect;
    [HideInInspector] public RectTransform leftDotRect;
    [HideInInspector] public RectTransform rightDotRect;
    [HideInInspector] public TextMeshProUGUI messageText;
    [HideInInspector] public Image barImage;
    [HideInInspector] public Image zoneImage;
    [HideInInspector] public Image[] pipImages = new Image[3];

    // Play area RectTransform — bar moves within this
    [HideInInspector] public RectTransform playArea;

    public float PlayAreaHeight => playArea != null ? playArea.rect.height : 600f;

    private static readonly Color ColActive  = new Color32(229, 184,  0, 255);
    private static readonly Color ColStopped = new Color32(100, 100,100, 255);
    private static readonly Color ColZone    = new Color32( 74, 222,128,  22);
    private static readonly Color ColPipOk   = new Color32( 74, 222,128, 255);
    private static readonly Color ColPipFail = new Color32(248, 113,113, 255);
    private static readonly Color ColPipIdle = new Color32( 50,  50, 50, 255);

    // ------------------------------------------------------------------
    public void SetRound(int round, float zoneTop, float zoneBot, float barH, float speed)
    {
        float h = PlayAreaHeight;

        float zoneHeight = (zoneBot - zoneTop) * h;
        float zoneCenter = ((zoneTop + zoneBot) * 0.5f * h) - (h / 2f);

        if (zoneRect != null)
        {
            zoneRect.sizeDelta = new Vector2(zoneRect.sizeDelta.x, zoneHeight);
            zoneRect.anchoredPosition = new Vector2(0f, zoneCenter);
        }

        if (leftDotRect != null)
            leftDotRect.anchoredPosition =
                new Vector2(leftDotRect.anchoredPosition.x, zoneCenter);

        if (rightDotRect != null)
            rightDotRect.anchoredPosition =
                new Vector2(rightDotRect.anchoredPosition.x, zoneCenter);

        if (barRect != null)
            barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, barH * h);

        if (barImage != null) barImage.color = ColActive;
        if (zoneImage != null) zoneImage.color = ColZone;
    }

    public void SetBarPosition(float posF)
    {
        if (barRect == null) return;

        float h = PlayAreaHeight;

        // 0 = bottom, 1 = top
        float y = (posF * h) - (h / 2f);

        barRect.anchoredPosition = new Vector2(0f, y);
    }

    public void ShowResult(bool hit)
    {
        if (barImage != null) barImage.color = hit ? ColPipOk : ColPipFail;
        if (zoneImage != null) zoneImage.color = hit
            ? new Color32(74, 222, 128, 50)
            : new Color32(248, 113, 113, 40);
    }

    public void SetPip(int index, bool success)
    {
        if (index < 0 || index >= pipImages.Length) return;
        if (pipImages[index] != null)
            pipImages[index].color = success ? ColPipOk : ColPipFail;
    }

    public void SetMessage(string text)
    {
        if (messageText != null) messageText.text = text;
    }

    public void ResetPips()
    {
        foreach (var p in pipImages)
            if (p != null) p.color = ColPipIdle;
    }
}
