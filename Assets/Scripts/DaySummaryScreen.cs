using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// DaySummaryScreen
/// =================
/// Shown at the end of each day. Displays stat changes for that day.
/// Built procedurally — no prefab needed.
/// Call Show() from DayEndHandler.HandleDayStart().
/// Player clicks DALEJ (Continue) to resume — callback fires to unpause.
/// </summary>
public class DaySummaryScreen : MonoBehaviour
{
    private GameObject canvas;
    private bool built = false;

    // UI refs
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI moneyFinalText;
    private TextMeshProUGUI moneyDiffText;
    private TextMeshProUGUI hipHopFinalText;
    private TextMeshProUGUI discoFinalText;
    private TextMeshProUGUI rockFinalText;
    private TextMeshProUGUI metalFinalText;
    private TextMeshProUGUI hipHopDiffText;
    private TextMeshProUGUI discoDiffText;
    private TextMeshProUGUI rockDiffText;
    private TextMeshProUGUI metalDiffText;

    private Action onContinue;

    // ------------------------------------------------------------------
    public void Show(
        int day,
        float finalMoney,  float moneyDiff,
        int hipHop,        int hipHopDiff,
        int disco,         int discoDiff,
        int rock,          int rockDiff,
        int metal,         int metalDiff,
        Action onContinueCallback = null)
    {
        if (!built) Build();

        onContinue = onContinueCallback;

        titleText.text      = $"KONIEC DNIA {day}";

        moneyFinalText.text = $"{finalMoney:F2}$";
        moneyDiffText.text  = FormatDiff(moneyDiff, "F2", "$");
        moneyDiffText.color = DiffColor(moneyDiff);

        hipHopFinalText.text = $"{hipHop}";
        discoFinalText.text  = $"{disco}";
        rockFinalText.text   = $"{rock}";
        metalFinalText.text  = $"{metal}";

        hipHopDiffText.text  = FormatDiff(hipHopDiff);
        discoDiffText.text   = FormatDiff(discoDiff);
        rockDiffText.text    = FormatDiff(rockDiff);
        metalDiffText.text   = FormatDiff(metalDiff);

        hipHopDiffText.color  = DiffColor(hipHopDiff);
        discoDiffText.color   = DiffColor(discoDiff);
        rockDiffText.color    = DiffColor(rockDiff);
        metalDiffText.color   = DiffColor(metalDiff);

        canvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (canvas != null) canvas.SetActive(false);
        Time.timeScale = 1f;
        onContinue?.Invoke();
    }

    // ------------------------------------------------------------------
    void Build()
    {
        var canvasGO = new GameObject("DaySummaryCanvas");
        var c = canvasGO.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 15;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        canvas = canvasGO;

        var ct = canvasGO.transform;

        // Dim overlay
        var overlay = MakeImage(ct, "Overlay", new Color(0f, 0f, 0f, 0.75f));
        StretchFull(overlay);

        // Main panel — wide and tall
        var panel = MakeImage(ct, "Panel", new Color32(18, 22, 28, 255));
        SR(panel, 0.5f, 0.5f, 860, 720, 0, 0);

        var border = MakeImage(panel.transform, "Border", new Color32(35, 50, 70, 255));
        SR(border, 0.5f, 0.5f, 840, 700, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // Title
        titleText = MakeText(ct, "Title", "KONIEC DNIA ?", 36,
                             new Color32(220, 180, 50, 255)).GetComponent<TextMeshProUGUI>();
        SR(titleText.gameObject, 0.5f, 0.5f, 820, 56, 0, 310);

        var subtitle = MakeText(ct, "Subtitle", "PODSUMOWANIE DNIA", 20,
                                new Color32(100, 120, 160, 255));
        SR(subtitle, 0.5f, 0.5f, 820, 34, 0, 262);

        // Top divider
        SR(MakeImage(ct, "Div0", new Color32(50, 65, 90, 255)), 0.5f, 0.5f, 780, 2, 0, 238);

        // Column headers
        MakeHeaderLabel(ct, "HdrStat",    "STATYSTYKA",  -300, 205);
        MakeHeaderLabel(ct, "HdrCurrent", "TERAZ",        100, 205);
        MakeHeaderLabel(ct, "HdrChange",  "ZMIANA",       340, 205);

        SR(MakeImage(ct, "Div1", new Color32(35, 48, 65, 255)), 0.5f, 0.5f, 780, 1, 0, 182);

        // Money row
        MakeRowLabel(ct, "MoneyLbl", "BUDŻET",     -300, 148);
        moneyFinalText = MakeValueText(ct, "MoneyFinal", "0.00$", 100, 148);
        moneyDiffText  = MakeDiffText (ct, "MoneyDiff",  "+0.00$", 340, 148);

        SR(MakeImage(ct, "Div2", new Color32(35, 48, 65, 255)), 0.5f, 0.5f, 780, 1, 0, 112);

        // Genre rows
        string[] genres    = { "HIP-HOP", "DISCO", "ROCK", "METAL" };
        float[]  yPositions = { 75, 20, -35, -90 };

        TextMeshProUGUI[] finals = new TextMeshProUGUI[4];
        TextMeshProUGUI[] diffs  = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            MakeRowLabel(ct, $"Genre_{i}", genres[i], -300, yPositions[i]);
            finals[i] = MakeValueText(ct, $"Final_{i}", "0",   100, yPositions[i]);
            diffs[i]  = MakeDiffText (ct, $"Diff_{i}",  "+0",  340, yPositions[i]);

            // Row separator (skip last)
            if (i < 3)
                SR(MakeImage(ct, $"DivR{i}", new Color32(25, 35, 50, 255)), 0.5f, 0.5f, 780, 1, 0, yPositions[i] - 27);
        }

        hipHopFinalText = finals[0]; discoFinalText = finals[1];
        rockFinalText   = finals[2]; metalFinalText = finals[3];
        hipHopDiffText  = diffs[0];  discoDiffText  = diffs[1];
        rockDiffText    = diffs[2];  metalDiffText  = diffs[3];

        SR(MakeImage(ct, "Div3", new Color32(50, 65, 90, 255)), 0.5f, 0.5f, 780, 2, 0, -128);

        // Continue button
        var continueBtn = MakeButton(ct, "ContinueBtn", "DALEJ →");
        SR(continueBtn, 0.5f, 0.5f, 260, 60, 0, -190);
        continueBtn.GetComponent<Button>().onClick.AddListener(Hide);

        canvas.SetActive(false);
        built = true;
    }

    // ------------------------------------------------------------------
    #region Helpers

    void MakeHeaderLabel(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 16, new Color32(80, 100, 140, 255));
        SR(go, 0.5f, 0.5f, 260, 30, ox, oy);
    }

    void MakeRowLabel(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 20, new Color32(160, 175, 210, 255));
        SR(go, 0.5f, 0.5f, 260, 38, ox, oy);
    }

    TextMeshProUGUI MakeValueText(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 22, new Color32(210, 225, 255, 255));
        SR(go, 0.5f, 0.5f, 220, 38, ox, oy);
        return go.GetComponent<TextMeshProUGUI>();
    }

    TextMeshProUGUI MakeDiffText(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 20, new Color32(80, 220, 100, 255));
        SR(go, 0.5f, 0.5f, 220, 38, ox, oy);
        return go.GetComponent<TextMeshProUGUI>();
    }

    string FormatDiff(float diff, string fmt = "F0", string suffix = "")
    {
        string sign = diff >= 0 ? "+" : "";
        return $"{sign}{diff.ToString(fmt)}{suffix}";
    }

    Color DiffColor(float diff)
    {
        if (diff > 0) return new Color32(80,  220, 100, 255);
        if (diff < 0) return new Color32(220, 80,  80,  255);
        return new Color32(140, 140, 140, 255);
    }

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

    GameObject MakeButton(Transform parent, string name, string label)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = new Color32(30, 55, 90, 255);
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color32(50, 85, 130, 255);
        cb.pressedColor     = new Color32(220, 180, 50, 255);
        btn.colors = cb;
        var lbl = MakeText(obj.transform, "Label", label, 22, new Color32(220, 190, 50, 255));
        SR(lbl, 0.5f, 0.5f, 260, 60, 0, 0);
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

    void StretchFull(GameObject obj)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    #endregion
}
