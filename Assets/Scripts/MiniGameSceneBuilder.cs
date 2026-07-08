using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// MiniGameSceneBuilder
/// =====================
/// Automatycznie buduje scenę minigry 4x4 z 4 przełącznikami narożnymi.
/// Dodaj do pustego GameObject i kliknij Play (lub użyj ContextMenu).
///
/// UKŁAD:
///
///   [Żółty ▲]   [  4x4 GRID  ]   [Zielony ▲]
///               [            ]
///               [            ]
///               [            ]
///   [Niebieski▲] [           ] [Fioletowy▲]
///
/// Przełączniki są w rogach, wizualnie przy siatce.
/// </summary>
public class MiniGameSceneBuilder : MonoBehaviour
{
    [Header("Opcje")]
    public bool buildOnStart = true;

    [Header("Wymiary")]
    public float cellSize  = 100f;
    public float cellGap   = 12f;
    public float switchSize = 90f;

    // Kolory przełączników
    static readonly Color32[] SwitchColors = new Color32[]
    {
        new Color32(220, 180, 20,  255), // Żółty
        new Color32(30,  160, 50,  255), // Zielony
        new Color32(30,  100, 220, 255), // Niebieski
        new Color32(140, 30,  180, 255), // Fioletowy
    };
    static readonly string[] SwitchNames = { "Żółty", "Zielony", "Niebieski", "Fioletowy" };

    // ------------------------------------------------------------------
    void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build MiniGame Scene")]
    public void Build(Transform parentOverride = null)
    {
        var oldCanvas = GameObject.Find("MiniGameCanvas");
        if (oldCanvas) DestroyImmediate(oldCanvas);
        var oldGM = GameObject.Find("MiniGameManager");
        if (oldGM) DestroyImmediate(oldGM);

        // ---- Canvas ----
        var canvasGO = new GameObject("MiniGameCanvas");
        // Jeśli podano rodzica (np. prefab Adaptera), canvas jest jego dzieckiem
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

        // ---- Panel tła (metalowa skrzynka) ----
        float gridW = 4 * cellSize + 3 * cellGap;
        float gridH = gridW;
        float panelW = gridW + switchSize * 2 + 60;
        float panelH = gridH + switchSize * 2 + 60;

        var bg = MakeImage(ct, "PanelBG", new Color32(28, 24, 20, 255));
        SR(bg, 0.5f, 0.5f, panelW + 60, panelH + 80, 0, 0);

        // Nakładka tekstury (ciemniejsza ramka)
        var border = MakeImage(bg.transform, "Border", new Color32(50, 44, 38, 255));
        SR(border, 0.5f, 0.5f, panelW + 40, panelH + 60, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // ---- Tytuł ----
        // Przesunięcie i wymiary, które są potrzebne dla przełączników i tytułu
        float hw = gridW / 2f + switchSize / 2f + 14f;
        float hh = gridH / 2f + switchSize / 2f + 14f;

        string newTitle = "Użyj przełączników, aby zapalić wszystkie lampki bezpieczników. Każdy przełącznik wpływa na obszar 3x3.";
        var title = MakeText(ct, "Title", newTitle, 30, new Color32(255, 128, 0, 255));
        var titleTmp = title.GetComponent<TextMeshProUGUI>();
        titleTmp.enableWordWrapping = true;
        
        TMP_FontAsset font = null;
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var f in fonts)
            if (f.name.Contains("Jersey10")) font = f;
        
#if UNITY_EDITOR
        if (font == null) font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Jersey10-Regular SDF.asset");
#endif
        if (font != null) titleTmp.font = font;

        // Tytuł mieści się pomiędzy przełącznikami i jest lekko ponad ich poziomem.
        float titleWidth = (hw * 2f) - switchSize - 20f;
        SR(title, 0.5f, 0.5f, titleWidth, 140, 0, hh + 20f);

        // ---- Status / HUD usunięte na prośbę użytkownika ----

        // ---- Grid 4x4 (centrum) ----
        var gridGO = new GameObject("LightsGrid");
        gridGO.transform.SetParent(ct, false);
        var gridRT = gridGO.AddComponent<RectTransform>();
        gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
        gridRT.pivot = new Vector2(0.5f, 0.5f);
        gridRT.sizeDelta = new Vector2(gridW, gridH);
        gridRT.anchoredPosition = Vector2.zero;
        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(cellSize, cellSize);
        glg.spacing  = new Vector2(cellGap, cellGap);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;
        glg.childAlignment = TextAnchor.UpperLeft;

        var lightImages = new Image[16];
        for (int i = 0; i < 16; i++)
        {
            var cell = MakeLightCell(gridGO.transform, i);
            lightImages[i] = cell.transform.Find("Bulb").GetComponent<Image>();
        }

        // ---- Przełączniki w rogach ----
        // Pozycje względem centrum gridu (wyliczone wcześniej)
        Vector2[] corners = {
            new Vector2(-hw,  hh),  // lewy górny  - Żółty
            new Vector2( hw,  hh),  // prawy górny - Zielony
            new Vector2(-hw, -hh),  // lewy dolny  - Niebieski
            new Vector2( hw, -hh),  // prawy dolny - Fioletowy
        };

        var switchBtns = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            var sw = MakeSwitchCell(ct, SwitchNames[i], SwitchColors[i]);
            SR(sw, 0.5f, 0.5f, switchSize, switchSize, corners[i].x, corners[i].y);
            switchBtns[i] = sw.GetComponent<Button>();
        }

        // Etykiety pod przełącznikami usunięte na prośbę użytkownika

        // ---- Linie wizualne (podgląd obszarów) - opcjonalne ozdobne prostokąty ----
        // Narysuj kolorowe obwódki na gridzie pokazujące jakie komórki zmienia który switch
        // (można wyłączyć - to tylko kosmetyka dla gracza)
        DrawRegionOverlay(ct, gridW, gridH);

        // ---- Game Manager ----
        var gmGO = new GameObject("MiniGameManager");
        gmGO.transform.SetParent(canvasGO.transform, false);
        var gm = gmGO.AddComponent<LightsOutMiniGame>();
        gm.lightImages   = lightImages;
        gm.switchYellow  = switchBtns[0];
        gm.switchGreen   = switchBtns[1];
        gm.switchBlue    = switchBtns[2];
        gm.switchPurple  = switchBtns[3];
        // Wyrzucone referencje do UI
        gm.shuffleCount  = 8;

        // Wywołaj Initialize zaraz po przypisaniu referencji
        // (nie czekaj na Start() — przyciski muszą działać od razu)
        gm.Initialize();

        Debug.Log("[MiniGameSceneBuilder] Scena minigry zbudowana!");
    }

    // ------------------------------------------------------------------
    void DrawRegionOverlay(Transform parent, float gridW, float gridH)
    {
        // Każdy region 3x3 to 3/4 szerokości gridu
        float regW = (3 * cellSize + 2 * cellGap);
        float regH = regW;
        float step = cellSize + cellGap;

        // Offsety lewego-górnego narożnika każdego regionu od centrum gridu
        // Grid zaczyna się od (-gridW/2, gridH/2) - lewy górny
        float x0 = -gridW / 2f;
        float y0 =  gridH / 2f;

        Vector2[] regionOffsets = {
            new Vector2(x0,         y0 - 0   ),  // żółty: wiersze 0-2, kol 0-2
            new Vector2(x0 + step,  y0 - 0   ),  // zielony: kol 1-3
            new Vector2(x0,         y0 - step),  // niebieski: wiersze 1-3
            new Vector2(x0 + step,  y0 - step),  // fioletowy: wiersze 1-3, kol 1-3
        };

        for (int i = 0; i < 4; i++)
        {
            var outline = new GameObject($"RegionOutline_{SwitchNames[i]}");
            outline.transform.SetParent(parent, false);
            var img = outline.AddComponent<Image>();

            // Obwódka (przezroczyste wypełnienie, tylko obramowanie przez alpha)
            Color c = new Color(SwitchColors[i].r/255f, SwitchColors[i].g/255f,
                                SwitchColors[i].b/255f, 0.18f);
            img.color = c;

            var rt = outline.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 1f); // lewy górny
            rt.sizeDelta = new Vector2(regW, regH);
            rt.anchoredPosition = regionOffsets[i];

            // Obwódka - zagnieżdżony element (outline trick)
            var inner = new GameObject("Inner");
            inner.transform.SetParent(outline.transform, false);
            var innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0,0,0,0); // przeźroczysty środek
            var innerRT = inner.GetComponent<RectTransform>();
            innerRT.anchorMin = Vector2.zero;
            innerRT.anchorMax = Vector2.one;
            innerRT.offsetMin = new Vector2(3, 3);
            innerRT.offsetMax = new Vector2(-3, -3);
        }
    }

    // ------------------------------------------------------------------
    #region Factory helpers

    GameObject MakeLightCell(Transform parent, int idx)
    {
        var obj = new GameObject($"Light_{idx:00}");
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.color = new Color32(35, 30, 25, 255); // Obudowa (kwadratowa)

        // Wewnętrzna żarówka
        var bulb = new GameObject("Bulb");
        bulb.transform.SetParent(obj.transform, false);
        var bi = bulb.AddComponent<Image>();
        bi.color = new Color32(63, 12, 0, 255);
        var brt = bulb.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.2f, 0.2f);
        brt.anchorMax = new Vector2(0.8f, 0.8f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        // Poświata (Glow)
        var glow = new GameObject("Glow");
        glow.transform.SetParent(bulb.transform, false);
        var glowImg = glow.AddComponent<Image>();
        glowImg.color = new Color32(255, 160, 50, 255); // Jasny pomarańcz
        var grt = glow.GetComponent<RectTransform>();
        grt.anchorMin = Vector2.zero;
        grt.anchorMax = Vector2.one;
        grt.offsetMin = new Vector2(-4, -4); // Minimalne wyjście poza żarówkę
        grt.offsetMax = new Vector2(4, 4);
        
        var ge = glow.AddComponent<GlowEffect>();
        ge.minAlpha = 0.1f;
        ge.maxAlpha = 0.5f;
        ge.minScale = 1.0f;
        ge.maxScale = 1.08f;

        return obj;
    }

    GameObject MakeSwitchCell(Transform parent, string label, Color32 color)
    {
        var obj = new GameObject($"Switch_{label}");
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();

        var bg = new GameObject("BG");
        bg.transform.SetParent(obj.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color32(35, 30, 25, 255);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        var btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = cb;
        btn.targetGraphic = bgImg;

        // Dźwignia
        var lever = new GameObject("Lever");
        lever.transform.SetParent(obj.transform, false);
        var leverImg = lever.AddComponent<Image>();
        leverImg.color = new Color(color.r/255f * 0.8f, color.g/255f * 0.8f, color.b/255f * 0.8f);
        var leverRT = lever.GetComponent<RectTransform>();
        leverRT.anchorMin = new Vector2(0.38f, 0.12f);
        leverRT.anchorMax = new Vector2(0.62f, 0.72f);
        leverRT.offsetMin = leverRT.offsetMax = Vector2.zero;
        leverRT.pivot     = new Vector2(0.5f, 0f);

        // Gałka
        var knob = new GameObject("Knob");
        knob.transform.SetParent(lever.transform, false);
        var knobImg = knob.AddComponent<Image>();
        knobImg.color = new Color(color.r/255f, color.g/255f, color.b/255f);
        var krt = knob.GetComponent<RectTransform>();
        krt.anchorMin = new Vector2(-0.6f, 0.72f);
        krt.anchorMax = new Vector2(1.6f,  1.05f);
        krt.offsetMin = krt.offsetMax = Vector2.zero;

        // LED
        var led = new GameObject("LED");
        led.transform.SetParent(obj.transform, false);
        var ledImg = led.AddComponent<Image>();
        ledImg.color = new Color(color.r/255f * 0.2f, color.g/255f * 0.2f, color.b/255f * 0.2f);
        var ledRT = led.GetComponent<RectTransform>();
        ledRT.anchorMin = new Vector2(0.72f, 0.76f);
        ledRT.anchorMax = new Vector2(0.92f, 0.96f);
        ledRT.offsetMin = ledRT.offsetMax = Vector2.zero;

        // SwitchLever
        var sl = obj.AddComponent<SwitchLever>();
        sl.leverTransform = leverRT;
        sl.ledIndicator   = ledImg;
        sl.ledOnColor     = new Color(color.r/255f, color.g/255f, color.b/255f);
        sl.ledOffColor    = new Color(color.r/255f*0.15f, color.g/255f*0.15f, color.b/255f*0.15f);
        sl.angleOff       = 22f;
        sl.angleOn        = -22f;
        sl.returnToOff    = true;
        sl.animTime       = 0.18f;
        btn.onClick.AddListener(() => sl.TriggerSwitch());

        return obj;
    }

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

    GameObject MakeButton(Transform parent, string name, string label)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = new Color32(50, 42, 34, 255);
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color32(80, 68, 52, 255);
        cb.pressedColor     = new Color32(180, 150, 40, 255);
        btn.colors = cb;

        var lbl = MakeText(obj.transform, "Label", label, 18, new Color32(220,190,50,255));
        SR(lbl, 0.5f, 0.5f, 200, 44, 0, 0);
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