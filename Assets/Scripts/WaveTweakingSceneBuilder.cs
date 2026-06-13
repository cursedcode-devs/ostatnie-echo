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
///     └── Content (przesunięty wyżej)
///         ├── PanelBG / Title
///         ├── GraphBox -> WaveGraph (wizualizacja: fala-cel + aktualna)
///         ├── AmpLabel / LenLabel / FreqLabel
///         ├── ActualValuesText  (TERAZ — bieżące wartości; CEL jest ukryty)
///         └── StatusText        (DOPASUJ FALE / SYGNAŁ STABILNY!)
///   WaveTweakingManager (WaveTweakingMiniGame component)
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

        // Kontener z całym UI — przesunięty trochę wyżej (ekran minigry wyświetla się wyżej).
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(canvasGO.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = Vector2.zero;
        contentRT.anchoredPosition = new Vector2(0f, 120f);
        var ct = content.transform;

        // ---- Background panel ----
        var bg = MakeImage(ct, "PanelBG", new Color32(18, 24, 20, 255));
        SR(bg, 0.5f, 0.5f, 760, 600, 0, 0);

        var border = MakeImage(bg.transform, "Border", new Color32(30, 60, 40, 255));
        SR(border, 0.5f, 0.5f, 740, 580, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // ---- Title ----
        var title = MakeText(ct, "Title", "KALIBRACJA SYGNAŁU", 30, new Color32(50, 220, 100, 255));
        SR(title, 0.5f, 0.5f, 700, 48, 0, 260);

        // ---- Wykres fal (cel + aktualna) ----
        var graphBox = MakeImage(ct, "GraphBox", new Color32(8, 14, 10, 255));
        SR(graphBox, 0.5f, 0.5f, 700, 340, 0, 65);

        var graphGO = new GameObject("WaveGraph");
        graphGO.transform.SetParent(graphBox.transform, false);
        var graphRT = graphGO.AddComponent<RectTransform>();
        graphRT.anchorMin = Vector2.zero;
        graphRT.anchorMax = Vector2.one;
        graphRT.offsetMin = new Vector2(8, 8);
        graphRT.offsetMax = new Vector2(-8, -8);
        var waveGraph = graphGO.AddComponent<WaveTweakingGraph>();
        waveGraph.lineThickness = 5f;

        // ---- Labels ----
        var ampLabel  = MakeText(ct, "AmpLabel",  "AMPLITUDA", 20, new Color32(150, 220, 150, 255));
        var lenLabel  = MakeText(ct, "LenLabel",  "DŁUGOŚĆ",   20, new Color32(150, 220, 150, 255));
        var freqLabel = MakeText(ct, "FreqLabel", "CZĘSTOTL.", 20, new Color32(150, 220, 150, 255));
        SR(ampLabel,  0.5f, 0.5f, 200, 30, -230, -130);
        SR(lenLabel,  0.5f, 0.5f, 200, 30,    0, -130);
        SR(freqLabel, 0.5f, 0.5f, 200, 30,  230, -130);

        // ---- Actual values (rząd CEL ukryty — wartości docelowe poznajesz po fali) ----
        var actLabel = MakeText(ct, "ActLabel", "TERAZ:", 18, new Color32(100, 180, 100, 255));
        SR(actLabel, 0.5f, 0.5f, 100, 30, -260, -185);

        var actValuesGO = MakeText(ct, "ActualValuesText", "5.0, 5.0, 5.0", 26, new Color32(200, 255, 200, 255));
        SR(actValuesGO, 0.5f, 0.5f, 520, 40, 30, -185);

        // ---- Status ----
        var statusGO = MakeText(ct, "StatusText", "DOPASUJ FALE", 24, Color.white);
        SR(statusGO, 0.5f, 0.5f, 600, 40, 0, -250);

        // ---- WaveTweakingMiniGame component ----
        var gmGO = new GameObject("WaveTweakingManager");
        gmGO.transform.SetParent(canvasGO.transform, false);
        var gm = gmGO.AddComponent<WaveTweakingMiniGame>();

        // Wire up UI script
        var uiScript = canvasGO.AddComponent<WaveTweakingMiniGameUI>();
        // Rząd CEL jest ukryty — wartości docelowe poznajesz po fali.
        uiScript.actualValuesText   = actValuesGO.GetComponent<TextMeshProUGUI>();
        uiScript.statusText         = statusGO.GetComponent<TextMeshProUGUI>();
        uiScript.waveGraph          = waveGraph;

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