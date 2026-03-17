using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// WaveTweakingSceneBuilder
/// =========================
/// Mirrors MiniGameSceneBuilder — procedurally builds the WaveTweaking UI at runtime.
/// Called by WaveTweakingMiniGameAdapter.BuildScene().
///
/// Builds:
///   WaveTweakingCanvas
///     ├── PanelBG
///     ├── Title
///     ├── RequiredValuesText   (target values)
///     ├── ActualValuesText     (current slider values)
///     ├── StatusText           (DOPASUJ FALE / SYGNAŁ STABILNY!)
///     └── WaveTweakingManager  (WaveTweakingMiniGame component)
/// </summary>
public class WaveTweakingSceneBuilder : MonoBehaviour
{
    [Header("Options")]
    public bool buildOnStart = true;

    void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build Wave Tweaking Scene")]
    public void Build(Transform parentOverride = null)
    {
        // Clean up any previous build
        var old = GameObject.Find("WaveTweakingCanvas");
        if (old) DestroyImmediate(old);
        var oldGM = GameObject.Find("WaveTweakingManager");
        if (oldGM) DestroyImmediate(oldGM);

        // ---- Canvas ----
        var canvasGO = new GameObject("WaveTweakingCanvas");
        if (parentOverride != null)
            canvasGO.transform.SetParent(parentOverride, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var ct = canvasGO.transform;

        // ---- Background panel ----
        var bg = MakeImage(ct, "PanelBG", new Color32(18, 24, 20, 255));
        SR(bg, 0.5f, 0.5f, 520, 320, 0, 0);

        var border = MakeImage(bg.transform, "Border", new Color32(30, 60, 40, 255));
        SR(border, 0.5f, 0.5f, 500, 300, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // ---- Title ----
        var title = MakeText(ct, "Title", "KALIBRACJA SYGNAŁU", 26, new Color32(50, 220, 100, 255));
        SR(title, 0.5f, 0.5f, 500, 44, 0, 120);

        // ---- Labels ----
        var ampLabel  = MakeText(ct, "AmpLabel",  "AMPLITUDA", 16, new Color32(150, 220, 150, 255));
        var lenLabel  = MakeText(ct, "LenLabel",  "DŁUGOŚĆ",   16, new Color32(150, 220, 150, 255));
        var freqLabel = MakeText(ct, "FreqLabel", "CZĘSTOTL.", 16, new Color32(150, 220, 150, 255));
        SR(ampLabel,  0.5f, 0.5f, 160, 30, -160, 60);
        SR(lenLabel,  0.5f, 0.5f, 160, 30,    0, 60);
        SR(freqLabel, 0.5f, 0.5f, 160, 30,  160, 60);

        // ---- Required values ----
        var reqLabel = MakeText(ct, "ReqLabel", "CEL:", 16, new Color32(100, 180, 100, 255));
        SR(reqLabel, 0.5f, 0.5f, 80, 30, -220, 20);

        var reqValuesGO = MakeText(ct, "RequiredValuesText", "-.-, -.-, -.-", 22, new Color32(50, 255, 120, 255));
        SR(reqValuesGO, 0.5f, 0.5f, 460, 36, 20, 20);

        // ---- Actual values ----
        var actLabel = MakeText(ct, "ActLabel", "TERAZ:", 16, new Color32(100, 180, 100, 255));
        SR(actLabel, 0.5f, 0.5f, 80, 30, -220, -20);

        var actValuesGO = MakeText(ct, "ActualValuesText", "5.0, 5.0, 5.0", 22, new Color32(200, 255, 200, 255));
        SR(actValuesGO, 0.5f, 0.5f, 460, 36, 20, -20);

        // ---- Status ----
        var statusGO = MakeText(ct, "StatusText", "DOPASUJ FALE", 20, Color.white);
        SR(statusGO, 0.5f, 0.5f, 400, 36, 0, -80);

        // ---- WaveTweakingMiniGame component ----
        var gmGO = new GameObject("WaveTweakingManager");
        gmGO.transform.SetParent(canvasGO.transform, false);
        var gm = gmGO.AddComponent<WaveTweakingMiniGame>();

        // Wire up UI script
        var uiScript = canvasGO.AddComponent<WaveTweakingMiniGameUI>();
        uiScript.requiredValuesText = reqValuesGO.GetComponent<TextMeshProUGUI>();
        uiScript.actualValuesText   = actValuesGO.GetComponent<TextMeshProUGUI>();
        uiScript.statusText         = statusGO.GetComponent<TextMeshProUGUI>();

        gm.uiScript = uiScript;

        // Note: sliders are injected after build by WaveTweakingMiniGameAdapter.SetSliders()

        Debug.Log("[WaveTweakingSceneBuilder] Scene built!");
    }

    // ------------------------------------------------------------------
    #region Helpers

    GameObject MakeImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = color;
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
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    #endregion
}