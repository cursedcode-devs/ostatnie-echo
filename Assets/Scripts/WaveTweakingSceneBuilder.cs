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
        var bg = MakeImage(ct, "PanelBG", new Color32(0, 0, 0, 255));
        SR(bg, 0.5f, 0.5f, 760, 600, 0, 0);

        var border = MakeImage(bg.transform, "Border", new Color32(63, 12, 0, 255));
        SR(border, 0.5f, 0.5f, 740, 580, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // ---- Title ----
        var title = MakeText(ct, "Title", "KALIBRACJA SYGNAŁU", 30, new Color32(255, 128, 0, 255));
        SR(title, 0.5f, 0.5f, 700, 48, 0, 260);

        // ---- Wykres fal (cel + aktualna) ----
        var graphBox = MakeImage(ct, "GraphBox", new Color32(0, 0, 0, 255));
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
        var ampLabel  = MakeText(ct, "AmpLabel",  "AMPLITUDA", 24, new Color32(255, 128, 0, 200));
        var lenLabel  = MakeText(ct, "LenLabel",  "DŁUGOŚĆ",   24, new Color32(255, 128, 0, 200));
        var freqLabel = MakeText(ct, "FreqLabel", "CZĘSTOTLIWOŚĆ", 24, new Color32(255, 128, 0, 200));
        SR(ampLabel,  0.5f, 0.5f, 200, 30, -230, -130);
        SR(lenLabel,  0.5f, 0.5f, 200, 30,    0, -130);
        SR(freqLabel, 0.5f, 0.5f, 200, 30,  230, -130);

        var ampVal = MakeText(ct, "AmpValueText", "5.0", 36, new Color32(255, 128, 0, 255));
        var lenVal = MakeText(ct, "LenValueText", "5.0", 36, new Color32(255, 128, 0, 255));
        var freqVal = MakeText(ct, "FreqValueText", "5.0", 36, new Color32(255, 128, 0, 255));
        SR(ampVal,  0.5f, 0.5f, 200, 40, -230, -170);
        SR(lenVal,  0.5f, 0.5f, 200, 40,    0, -170);
        SR(freqVal, 0.5f, 0.5f, 200, 40,  230, -170);

        var ampTargetVal = MakeText(ct, "AmpTargetText", "5.0", 24, new Color32(255, 128, 0, 150));
        var lenTargetVal = MakeText(ct, "LenTargetText", "5.0", 24, new Color32(255, 128, 0, 150));
        var freqTargetVal = MakeText(ct, "FreqTargetText", "5.0", 24, new Color32(255, 128, 0, 150));
        SR(ampTargetVal,  0.5f, 0.5f, 200, 30, -230, -205);
        SR(lenTargetVal,  0.5f, 0.5f, 200, 30,    0, -205);
        SR(freqTargetVal, 0.5f, 0.5f, 200, 30,  230, -205);

        // ---- Status ----
        var statusGO = MakeText(ct, "StatusText", "Użyj suwaków poniżej, aby dopasować obie fale sygnału.", 26, new Color32(255, 128, 0, 255));
        SR(statusGO, 0.5f, 0.5f, 720, 80, 0, -260);

        // ---- WaveTweakingMiniGame component ----
        var gmGO = new GameObject("WaveTweakingManager");
        gmGO.transform.SetParent(canvasGO.transform, false);
        var gm = gmGO.AddComponent<WaveTweakingMiniGame>();

        // Wire up UI script
        var uiScript = canvasGO.AddComponent<WaveTweakingMiniGameUI>();
        // Rząd CEL jest ukryty — wartości docelowe poznajesz po fali.
        uiScript.ampValueText       = ampVal.GetComponent<TextMeshProUGUI>();
        uiScript.lenValueText       = lenVal.GetComponent<TextMeshProUGUI>();
        uiScript.freqValueText      = freqVal.GetComponent<TextMeshProUGUI>();
        uiScript.ampTargetText      = ampTargetVal.GetComponent<TextMeshProUGUI>();
        uiScript.lenTargetText      = lenTargetVal.GetComponent<TextMeshProUGUI>();
        uiScript.freqTargetText     = freqTargetVal.GetComponent<TextMeshProUGUI>();
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

        TMP_FontAsset font = null;
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var f in fonts)
            if (f.name.Contains("Jersey10")) font = f;
        
#if UNITY_EDITOR
        if (font == null) font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Jersey10-Regular SDF.asset");
#endif
        if (font != null) tmp.font = font;

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