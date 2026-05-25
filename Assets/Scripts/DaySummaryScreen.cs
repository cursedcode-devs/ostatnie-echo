using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

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
    private TextMeshProUGUI kawalerka_feeText;
    private TextMeshProUGUI kawalerka_jedzenieText;
    private TextMeshProUGUI kawalerka_studiaText;
    private TextMeshProUGUI moneyFinalText;
    private TextMeshProUGUI moneyDiffText;
    private TextMeshProUGUI hipHopFinalText;
    private TextMeshProUGUI discoFinalText;
    private TextMeshProUGUI rockFinalText;
    private TextMeshProUGUI popFinalText;
    private TextMeshProUGUI hipHopDiffText;
    private TextMeshProUGUI discoDiffText;
    private TextMeshProUGUI rockDiffText;
    private TextMeshProUGUI popDiffText;
    private TextMeshProUGUI adsPenaltyText;
    private TextMeshProUGUI adsPenaltyBreakdownText;

    private Action onContinue;

    // ------------------------------------------------------------------
    public void Show(
        int day,
        float kawalerka_fee,
        float jedzenie_fee,
        float studia_fee,
        float adsPenalty,
        List<AdContractManager.UnplayedAdPenalty> unplayedPenalties,
        float finalMoney, float moneyDiff,
        int hipHop, int hipHopDiff,
        int disco, int discoDiff,
        int rock, int rockDiff,
        int pop, int popDiff,
        Action onContinueCallback = null)
    {
        if (!built) Build();

        onContinue = onContinueCallback;

        titleText.text = $"KONIEC DNIA {day - 1}";

        kawalerka_feeText.text = $"{kawalerka_fee:F2}$";
        kawalerka_jedzenieText.text = $"{jedzenie_fee:F2}$";
        kawalerka_studiaText.text = $"{studia_fee:F2}$";
        if (adsPenaltyText != null)
        {
            adsPenaltyText.text = $"{adsPenalty:F2}$";
        }
        
        // Wyświetlanie szczegółowej listy kar
        if (adsPenaltyBreakdownText != null)
        {
            if (unplayedPenalties != null && unplayedPenalties.Count > 0)
            {
                string bText = "Niewyemitowane zlecenia (kara 1/2 zysku):\n";
                foreach (var p in unplayedPenalties)
                {
                    bText += $" • {p.clientName} (\"{p.adTitle}\"): -{p.penaltyAmount:F2}$\n";
                }
                adsPenaltyBreakdownText.text = bText;
                adsPenaltyBreakdownText.gameObject.SetActive(true);
            }
            else
            {
                adsPenaltyBreakdownText.text = "";
                adsPenaltyBreakdownText.gameObject.SetActive(false);
            }
        }
        
        moneyFinalText.text = $"{finalMoney:F2}$";
        moneyDiffText.text = FormatDiff(moneyDiff, "F2", "$");
        moneyDiffText.color = DiffColor(moneyDiff);

        hipHopFinalText.text = $"{hipHop}";
        discoFinalText.text = $"{disco}";
        rockFinalText.text = $"{rock}";
        popFinalText.text = $"{pop}";

        hipHopDiffText.text = FormatDiff(hipHopDiff);
        discoDiffText.text = FormatDiff(discoDiff);
        rockDiffText.text = FormatDiff(rockDiff);
        popDiffText.text = FormatDiff(popDiff);

        hipHopDiffText.color = DiffColor(hipHopDiff);
        discoDiffText.color = DiffColor(discoDiff);
        rockDiffText.color = DiffColor(rockDiff);
        popDiffText.color = DiffColor(popDiff);

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
    c.renderMode = RenderMode.ScreenSpaceOverlay;
    c.sortingOrder = 15;

    var scaler = canvasGO.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);

    canvasGO.AddComponent<GraphicRaycaster>();
    canvas = canvasGO;

    var ct = canvasGO.transform;

    // Overlay
    var overlay = MakeImage(ct, "Overlay", new Color(0f, 0f, 0f, 0.75f));
    StretchFull(overlay);

    // 🔥 WIĘKSZY PANEL
    var panel = MakeImage(ct, "Panel", new Color32(18, 22, 28, 255));
    SR(panel, 0.5f, 0.5f, 860, 860, 0, 0);

    var border = MakeImage(panel.transform, "Border", new Color32(35, 50, 70, 255));
    SR(border, 0.5f, 0.5f, 840, 840, 0, 0);
    border.GetComponent<RectTransform>().SetAsFirstSibling();

    // Title
    titleText = MakeText(ct, "Title", "KONIEC DNIA ?", 36,
        new Color32(220, 180, 50, 255)).GetComponent<TextMeshProUGUI>();
    SR(titleText.gameObject, 0.5f, 0.5f, 820, 56, 0, 360);

    var subtitle = MakeText(ct, "Subtitle", "PODSUMOWANIE DNIA", 20,
        new Color32(100, 120, 160, 255));
    SR(subtitle, 0.5f, 0.5f, 820, 34, 0, 315);

    SR(MakeImage(ct, "Div0", new Color32(50, 65, 90, 255)),
        0.5f, 0.5f, 780, 2, 0, 290);

    // Headers
    MakeHeaderLabel(ct, "HdrStat", "STATYSTYKA", -300, 255);
    MakeHeaderLabel(ct, "HdrCurrent", "TERAZ", 100, 255);
    MakeHeaderLabel(ct, "HdrChange", "ZMIANA", 340, 255);

    SR(MakeImage(ct, "Div1", new Color32(35, 48, 65, 255)),
        0.5f, 0.5f, 780, 1, 0, 230);


    float y = 190;
    float step = 45;

    MakeRowLabel(ct, "FeeRentLbl", "CZYNSZ", -300, y);
    kawalerka_feeText = MakeValueText(ct, "FeeRentVal", "0.00$", 100, y);
    kawalerka_feeText.color = Color.red;

    y -= step;
    MakeRowLabel(ct, "FeeFoodLbl", "JEDZENIE", -300, y);
    kawalerka_jedzenieText = MakeValueText(ct, "FeeFoodVal", "0.00$", 100, y);
    kawalerka_jedzenieText.color = Color.red;

    y -= step;
    MakeRowLabel(ct, "FeeStudyLbl", "STUDIA", -300, y);
    kawalerka_studiaText = MakeValueText(ct, "FeeStudyVal", "0.00$", 100, y);
    kawalerka_studiaText.color = Color.red;

    y -= step;
    MakeRowLabel(ct, "FeeAdsPenaltyLbl", "KARY ZA REKLAMY", -300, y);
    adsPenaltyText = MakeValueText(ct, "FeeAdsPenaltyVal", "0.00$", 100, y);
    adsPenaltyText.color = Color.red;

    // Miejsce na listę kar z reklam
    var breakdownGO = MakeText(ct, "FeeAdsPenaltyBreakdown", "", 14, new Color32(200, 110, 110, 255));
    adsPenaltyBreakdownText = breakdownGO.GetComponent<TextMeshProUGUI>();
    adsPenaltyBreakdownText.alignment = TextAlignmentOptions.TopLeft;
    SR(breakdownGO, 0.5f, 0.5f, 780, 70, 0, y - 45);

    // separator przesunięty w dół o 60 pikseli, aby zrobić miejsce na tekst listy
    y -= 75;
    SR(MakeImage(ct, "DivFees", new Color32(35, 48, 65, 255)),
        0.5f, 0.5f, 780, 1, 0, y);



    y -= 50;
    MakeRowLabel(ct, "MoneyLbl", "BUDŻET", -300, y);
    moneyFinalText = MakeValueText(ct, "MoneyFinal", "0.00$", 100, y);
    moneyDiffText = MakeDiffText(ct, "MoneyDiff", "+0.00$", 340, y);

    // separator
    y -= 40;
    SR(MakeImage(ct, "Div2", new Color32(35, 48, 65, 255)),
        0.5f, 0.5f, 780, 1, 0, y);



    string[] genres = { "HIP-HOP", "DISCO", "ROCK", "METAL" };

    TextMeshProUGUI[] finals = new TextMeshProUGUI[4];
    TextMeshProUGUI[] diffs = new TextMeshProUGUI[4];

    for (int i = 0; i < 4; i++)
    {
        y -= 45;

        MakeRowLabel(ct, $"Genre_{i}", genres[i], -300, y);
        finals[i] = MakeValueText(ct, $"Final_{i}", "0", 100, y);
        diffs[i] = MakeDiffText(ct, $"Diff_{i}", "+0", 340, y);

        if (i < 3)
        {
            SR(MakeImage(ct, $"DivR{i}", new Color32(25, 35, 50, 255)),
                0.5f, 0.5f, 780, 1, 0, y - 25);
        }
    }

    hipHopFinalText = finals[0];
    discoFinalText = finals[1];
    rockFinalText = finals[2];
    popFinalText = finals[3];

    hipHopDiffText = diffs[0];
    discoDiffText = diffs[1];
    rockDiffText = diffs[2];
    popDiffText = diffs[3];

    // Bottom divider
    y -= 40;
    SR(MakeImage(ct, "Div3", new Color32(50, 65, 90, 255)),
        0.5f, 0.5f, 780, 2, 0, y);

    // =========================
    // ▶ BUTTON
    // =========================

    var continueBtn = MakeButton(ct, "ContinueBtn", "DALEJ →");
    SR(continueBtn, 0.5f, 0.5f, 260, 60, 0, y - 70);

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
        img.color = new Color32(30, 55, 90, 255);
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color32(50, 85, 130, 255);
        cb.pressedColor = new Color32(220, 180, 50, 255);
        btn.colors = cb;
        var lbl = MakeText(obj.transform, "Label", label, 22, new Color32(220, 190, 50, 255));
        SR(lbl, 0.5f, 0.5f, 260, 60, 0, 0);
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
