using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CableFixSceneBuilder
/// =====================
/// Procedurally builds the CableFix minigame UI at runtime.
/// Called by CableFixMiniGameAdapter.BuildScene().
///
/// Layout:
///   CableFixCanvas
///     BG panel (dark)
///       PlayArea (where bar moves)
///         LeftCable  (vertical line + dot)
///         RightCable (vertical line + dot)
///         ZoneRect   (green tinted area between dots)
///         Bar        (yellow moving strip)
///       HUD (pips + message at bottom)
/// </summary>
public class CableFixSceneBuilder : MonoBehaviour
{
    [Header("Options")]
    public bool buildOnStart = true;

    void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build CableFix Scene")]
    public void Build(Transform parentOverride = null)
    {
        var old = GameObject.Find("CableFixCanvas");
        if (old) DestroyImmediate(old);

        // ---- Canvas ----
        var canvasGO = new GameObject("CableFixCanvas");
        if (parentOverride != null)
            canvasGO.transform.SetParent(parentOverride, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var ct = canvasGO.transform;

        // ---- BG panel ----
        var bg = MakeImage(ct, "BG", new Color32(14, 14, 18, 255));
        SR(bg, 0.5f, 0.5f, 740, 560, 0, 0);

        var border = MakeImage(bg.transform, "Border", new Color32(30, 32, 44, 255));
        SR(border, 0.5f, 0.5f, 720, 540, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // ---- Title ----
        var title = MakeText(ct, "Title", "NAPRAWA SYGNAŁU", 24, new Color32(180, 160, 60, 255));
        SR(title, 0.5f, 0.5f, 700, 40, 0, 230);

        // ---- Play area ----
        var playAreaGO = new GameObject("PlayArea");
        playAreaGO.transform.SetParent(ct, false);
        var playRT = playAreaGO.AddComponent<RectTransform>();
        playRT.anchorMin = playRT.anchorMax = new Vector2(0.5f, 0.5f);
        playRT.pivot     = new Vector2(0.5f, 0.5f);
        playRT.sizeDelta = new Vector2(660, 420);
        playRT.anchoredPosition = new Vector2(0, 10);
        playAreaGO.AddComponent<CanvasGroup>();

        var playArea = playRT;

        float pw = 660f, ph = 420f;
        float cableX = pw / 2f - 30f;   // left/right cable x offset from play area centre

        // ---- Left cable ----
        var leftCableImg = MakeImage(playAreaGO.transform, "LeftCable", new Color32(70, 70, 80, 255));
        var lcRT = leftCableImg.GetComponent<RectTransform>();
        lcRT.anchorMin = lcRT.anchorMax = new Vector2(0.5f, 0.5f);
        lcRT.pivot     = new Vector2(0.5f, 0.5f);
        lcRT.sizeDelta = new Vector2(4, ph);
        lcRT.anchoredPosition = new Vector2(-cableX, 0);

        // ---- Right cable ----
        var rightCableImg = MakeImage(playAreaGO.transform, "RightCable", new Color32(70, 70, 80, 255));
        var rcRT = rightCableImg.GetComponent<RectTransform>();
        rcRT.anchorMin = rcRT.anchorMax = new Vector2(0.5f, 0.5f);
        rcRT.pivot     = new Vector2(0.5f, 0.5f);
        rcRT.sizeDelta = new Vector2(4, ph);
        rcRT.anchoredPosition = new Vector2(cableX, 0);

        // ---- Zone rect ----
        var zoneGO = MakeImage(playAreaGO.transform, "Zone", new Color32(74, 222, 128, 22));
        var zoneRT = zoneGO.GetComponent<RectTransform>();
        zoneRT.anchorMin = zoneRT.anchorMax = new Vector2(0.5f, 0.5f);
        zoneRT.pivot     = new Vector2(0.5f, 0.5f);
        zoneRT.sizeDelta = new Vector2(pw - 10, 100);
        zoneRT.anchoredPosition = Vector2.zero;

        // Zone border lines (top + bottom)
        var zoneTopLine = MakeImage(zoneGO.transform, "ZoneTop", new Color32(74, 222, 128, 90));
        var ztRT = zoneTopLine.GetComponent<RectTransform>();
        ztRT.anchorMin = new Vector2(0, 1); ztRT.anchorMax = new Vector2(1, 1);
        ztRT.pivot = new Vector2(0.5f, 1f);
        ztRT.offsetMin = new Vector2(0, -2); ztRT.offsetMax = Vector2.zero;

        var zoneBotLine = MakeImage(zoneGO.transform, "ZoneBot", new Color32(74, 222, 128, 90));
        var zbRT = zoneBotLine.GetComponent<RectTransform>();
        zbRT.anchorMin = new Vector2(0, 0); zbRT.anchorMax = new Vector2(1, 0);
        zbRT.pivot = new Vector2(0.5f, 0f);
        zbRT.offsetMin = Vector2.zero; zbRT.offsetMax = new Vector2(0, 2);

        // ---- Left dot ----
        var leftDotGO = MakeImage(playAreaGO.transform, "LeftDot", new Color32(229, 184, 0, 255));
        var ldRT = leftDotGO.GetComponent<RectTransform>();
        ldRT.anchorMin = ldRT.anchorMax = new Vector2(0.5f, 0.5f);
        ldRT.pivot = new Vector2(0.5f, 0.5f);
        ldRT.sizeDelta = new Vector2(20, 20);
        ldRT.anchoredPosition = new Vector2(-cableX, 0);
        leftDotGO.GetComponent<Image>().color = new Color32(229, 184, 0, 255);

        // ---- Right dot ----
        var rightDotGO = MakeImage(playAreaGO.transform, "RightDot", new Color32(229, 184, 0, 255));
        var rdRT = rightDotGO.GetComponent<RectTransform>();
        rdRT.anchorMin = rdRT.anchorMax = new Vector2(0.5f, 0.5f);
        rdRT.pivot = new Vector2(0.5f, 0.5f);
        rdRT.sizeDelta = new Vector2(20, 20);
        rdRT.anchoredPosition = new Vector2(cableX, 0);
        rightDotGO.GetComponent<Image>().color = new Color32(229, 184, 0, 255);

        // Round dots into circles using aspect ratio fitter
        foreach (var dot in new GameObject[]{ leftDotGO, rightDotGO })
        {
            var fitter = dot.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = 1f;
        }

        // ---- Bar ----
        var barGO = MakeImage(playAreaGO.transform, "Bar", new Color32(229, 184, 0, 255));
        var barRT = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = barRT.anchorMax = new Vector2(0.5f, 0.5f);
        barRT.pivot     = new Vector2(0.5f, 0.5f);
        barRT.sizeDelta = new Vector2(pw - 10, 75);
        barRT.anchoredPosition = Vector2.zero;

        var barLabel = MakeText(barGO.transform, "BarLabel", "OBSZAR SYGNAŁU", 13,
                                new Color32(65, 36, 2, 255));
        SR(barLabel, 0.5f, 0.5f, 300, 30, 0, 0);

        // ---- HUD ----
        var hud = MakeImage(ct, "HUD", new Color32(20, 20, 26, 255));
        SR(hud, 0.5f, 0.5f, 740, 54, 0, -240);

        var hudBorder = MakeImage(hud.transform, "HUDBorder", new Color32(35, 35, 48, 255));
        SR(hudBorder, 0.5f, 0.5f, 720, 2, 0, 26);

        // Pips
        var pipImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var pip = MakeImage(hud.transform, $"Pip_{i}", new Color32(50, 50, 50, 255));
            SR(pip, 0.5f, 0.5f, 18, 18, -30 + i * 26 - 260f, 0);
            var af = pip.AddComponent<AspectRatioFitter>();
            af.aspectMode  = AspectRatioFitter.AspectMode.HeightControlsWidth;
            af.aspectRatio = 1f;
            pipImages[i] = pip.GetComponent<Image>();
        }

        var msgGO = MakeText(hud.transform, "Message", "kliknij aby zacząć", 15,
                             new Color32(120, 120, 130, 255));
        SR(msgGO, 0.5f, 0.5f, 340, 40, 30, 0);

        var hintGO = MakeText(hud.transform, "Hint", "[ spacja ] zatrzymaj", 12,
                              new Color32(70, 70, 80, 255));
        SR(hintGO, 0.5f, 0.5f, 200, 40, 240, 0);

        // ---- Wire up game + UI ----
        var gmGO = new GameObject("CableFixManager");
        gmGO.transform.SetParent(canvasGO.transform, false);

        var game = gmGO.AddComponent<CableFixMiniGame>();
        var ui   = gmGO.AddComponent<CableFixUI>();

        ui.playArea     = playArea;
        ui.barRect      = barRT;
        ui.barImage     = barGO.GetComponent<Image>();
        ui.zoneRect     = zoneRT;
        ui.zoneImage    = zoneGO.GetComponent<Image>();
        ui.leftDotRect  = ldRT;
        ui.rightDotRect = rdRT;
        ui.messageText  = msgGO.GetComponent<TextMeshProUGUI>();
        ui.pipImages    = pipImages;
        ui.ResetPips();

        game.uiScript = ui;

        Debug.Log("[CableFixSceneBuilder] Scene built!");
    }

    // ------------------------------------------------------------------
    #region Helpers

    GameObject MakeImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<Image>().color = color;
        return obj;
    }

    GameObject MakeText(Transform parent, string name, string text, int size, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return obj;
    }

    void SR(GameObject obj, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = obj.GetComponent<RectTransform>();
        if (!rt) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    #endregion
}
