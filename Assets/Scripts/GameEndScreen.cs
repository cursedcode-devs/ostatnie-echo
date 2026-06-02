using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameEndScreen
/// =============
/// Shown when the game ends. Displays final stats and last-day changes.
/// Built procedurally — no prefab needed.
/// Call Show() from DayEndHandler.HandleGameFinished().
/// </summary>
public class GameEndScreen : MonoBehaviour
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

    // ------------------------------------------------------------------
    public void Show(
        int endGameCause, int totalDays,
        float finalMoney, float moneyDiff,
        int hipHop, int hipHopDiff,
        int disco, int discoDiff,
        int rock, int rockDiff,
        int pop, int popDiff)
    {
        if (!built) Build();

        switch (endGameCause)
        {
            default:
                titleText.text = $"KONIEC GRY — {totalDays - 1} DNI";
                break;
            case 0:
                //Game is won
                titleText.text = $"WYGRAŁEŚ! KONIEC GRY — {totalDays - 1} DNI";
                break;
            case 1:
                //Game Lost - no money
                titleText.text = $"PRZEGRAŁEŚ! NIE MASZ PIENIĘDZY! KONIEC GRY — {totalDays - 1} DNI";
                break;
            case 2:
                titleText.text = $"PRZEGRAŁEŚ! ZA MAŁO SŁUCHACZY! KONIEC GRY — {totalDays - 1} DNI";
                //Game Lost - not enough listeners
                break;
        }

        moneyFinalText.text = $"{finalMoney:F2}$";
        moneyDiffText.text = FormatDiff(moneyDiff, "F2", "$");
        moneyDiffText.color = DiffColor(moneyDiff);

        hipHopFinalText.text = $"{hipHop}";
        discoFinalText.text = $"{disco}";
        rockFinalText.text = $"{rock}";
        metalFinalText.text = $"{pop}";

        hipHopDiffText.text = FormatDiff(hipHopDiff);
        discoDiffText.text = FormatDiff(discoDiff);
        rockDiffText.text = FormatDiff(rockDiff);
        metalDiffText.text = FormatDiff(popDiff);

        hipHopDiffText.color = DiffColor(hipHopDiff);
        discoDiffText.color = DiffColor(discoDiff);
        rockDiffText.color = DiffColor(rockDiff);
        metalDiffText.color = DiffColor(popDiff);

        canvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (canvas != null) canvas.SetActive(false);
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if UNITY_STANDALONE
        Application.Quit();
#endif
    }

    // ------------------------------------------------------------------
    void Build()
    {
        var canvasGO = new GameObject("GameEndCanvas");
        var c = canvasGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 20;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        canvas = canvasGO;

        var ct = canvasGO.transform;

        // Full-screen dim overlay
        var overlay = MakeImage(ct, "Overlay", new Color(0f, 0f, 0f, 0.9f));
        StretchFull(overlay);

        // Main panel
        var panel = MakeImage(ct, "Panel", new Color32(16, 18, 22, 255));
        SR(panel, 0.5f, 0.5f, 900, 760, 0, 0);

        var border = MakeImage(panel.transform, "Border", new Color32(80, 55, 20, 255));
        SR(border, 0.5f, 0.5f, 880, 740, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // Title
        titleText = MakeText(ct, "Title", "KONIEC GRY", 42,
                             new Color32(240, 190, 40, 255)).GetComponent<TextMeshProUGUI>();
        SR(titleText.gameObject, 0.5f, 0.5f, 860, 60, 0, 328);

        var subtitle = MakeText(ct, "Subtitle", "WYNIKI KOŃCOWE", 20,
                                new Color32(160, 130, 60, 255));
        SR(subtitle, 0.5f, 0.5f, 860, 34, 0, 278);

        SR(MakeImage(ct, "Div0", new Color32(80, 60, 20, 255)), 0.5f, 0.5f, 820, 2, 0, 252);

        // Column headers
        MakeHeaderLabel(ct, "HdrStat", "STATYSTYKA", -320, 218);
        MakeHeaderLabel(ct, "HdrFinal", "WYNIK KOŃCOWY", 100, 218);
        MakeHeaderLabel(ct, "HdrLastDay", "ZMIANA (OSTATNI DZIEŃ)", 360, 218);

        SR(MakeImage(ct, "Div1", new Color32(45, 38, 18, 255)), 0.5f, 0.5f, 820, 1, 0, 194);

        // Money row
        MakeRowLabel(ct, "MoneyLbl", "BUDŻET", -320, 158);
        moneyFinalText = MakeValueText(ct, "MoneyFinal", "0.00$", 100, 158);
        moneyDiffText = MakeDiffText(ct, "MoneyDiff", "+0.00$", 360, 158);

        SR(MakeImage(ct, "Div2", new Color32(45, 38, 18, 255)), 0.5f, 0.5f, 820, 1, 0, 120);

        // Genre rows
        string[] genres = { "HIP-HOP", "DISCO", "ROCK", "METAL" };
        float[] yPos = { 82, 22, -38, -98 };

        TextMeshProUGUI[] finals = new TextMeshProUGUI[4];
        TextMeshProUGUI[] diffs = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            MakeRowLabel(ct, $"Genre_{i}", genres[i], -320, yPos[i]);
            finals[i] = MakeValueText(ct, $"Final_{i}", "0", 100, yPos[i]);
            diffs[i] = MakeDiffText(ct, $"Diff_{i}", "+0", 360, yPos[i]);
            if (i < 3)
                SR(MakeImage(ct, $"DivR{i}", new Color32(30, 25, 12, 255)), 0.5f, 0.5f, 820, 1, 0, yPos[i] - 30);
        }

        hipHopFinalText = finals[0]; discoFinalText = finals[1];
        rockFinalText = finals[2]; metalFinalText = finals[3];
        hipHopDiffText = diffs[0]; discoDiffText = diffs[1];
        rockDiffText = diffs[2]; metalDiffText = diffs[3];

        SR(MakeImage(ct, "Div3", new Color32(80, 60, 20, 255)), 0.5f, 0.5f, 820, 2, 0, -148);

        // Close button
        var closeBtn = MakeButton(ct, "CloseBtn", "ZAKOŃCZ");
        SR(closeBtn, 0.5f, 0.5f, 280, 64, 0, -214);
        closeBtn.GetComponent<Button>().onClick.AddListener(Hide);

        canvas.SetActive(false);
        built = true;
    }

    // ------------------------------------------------------------------
    #region Helpers

    void MakeHeaderLabel(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 15, new Color32(120, 100, 50, 255));
        SR(go, 0.5f, 0.5f, 300, 30, ox, oy);
    }

    void MakeRowLabel(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 22, new Color32(190, 170, 120, 255));
        SR(go, 0.5f, 0.5f, 280, 40, ox, oy);
    }

    TextMeshProUGUI MakeValueText(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 24, new Color32(230, 220, 200, 255));
        SR(go, 0.5f, 0.5f, 240, 40, ox, oy);
        return go.GetComponent<TextMeshProUGUI>();
    }

    TextMeshProUGUI MakeDiffText(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 22, new Color32(80, 220, 100, 255));
        SR(go, 0.5f, 0.5f, 280, 40, ox, oy);
        return go.GetComponent<TextMeshProUGUI>();
    }

    string FormatDiff(float diff, string fmt = "F0", string suffix = "")
    {
        string sign = diff >= 0 ? "+" : "";
        return $"{sign}{diff.ToString(fmt)}{suffix}";
    }

    Color DiffColor(float diff)
    {
        if (diff > 0) return new Color32(80, 220, 100, 255);
        if (diff < 0) return new Color32(220, 80, 80, 255);
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
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return obj;
    }

    GameObject MakeButton(Transform parent, string name, string label)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = new Color32(60, 40, 10, 255);
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color32(90, 65, 20, 255);
        cb.pressedColor = new Color32(240, 190, 40, 255);
        btn.colors = cb;
        var lbl = MakeText(obj.transform, "Label", label, 22, new Color32(240, 190, 40, 255));
        SR(lbl, 0.5f, 0.5f, 280, 64, 0, 0);
        return obj;
    }

    void SR(GameObject obj, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = obj.GetComponent<RectTransform>();
        if (!rt) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
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
