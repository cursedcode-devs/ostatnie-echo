using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WaveTweakingGraph — wizualizacja minigry „dostrajanie fali".
/// Rysuje dwie sinusoidy na jednym wykresie UI:
///   • CEL      — fala, w którą celuje gracz (przygaszony kolor),
///   • AKTUALNA — fala wynikająca z obecnych ustawień sliderów (jaskrawy kolor).
/// Gdy gracz dopasuje wszystkie trzy slidery, fale się pokrywają.
///
/// Trzy parametry (raw 0-1, jak w WaveTweakingMiniGame) mapują się na falę tak, by
/// "co widać = to się liczy" — każda cecha jest niezależna, zawsze widoczna i jednoznaczna:
///   amplituda    -> wysokość oscylacji,
///   częstotliwość-> liczba cykli (zawsze >= minCycles, więc oscylacja zawsze widoczna),
///   długość      -> pionowe przesunięcie całej fali (bez fazy = bez okresowej dwuznaczności).
///
/// Komponent jest samodzielnym UI Graphic (rysuje siatkę w OnPopulateMesh),
/// więc nie wymaga żadnego sprite'a ani dodatkowych zależności.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class WaveTweakingGraph : MaskableGraphic
{
    [Header("Wygląd")]
    [Tooltip("Kolor fali-celu (wyraźny, kontrastowy).")]
    public Color targetColor = new Color(1f, 0.5f, 0f, 0.4f);
    [Tooltip("Kolor fali aktualnej (jaskrawy).")]
    public Color currentColor = new Color(1f, 0.5f, 0f, 1f);
    [Tooltip("Kolor fali aktualnej, gdy idealnie dopasowana do celu.")]
    public Color matchedColor = new Color(0.29f, 1f, 0.5f, 1f);
    [Tooltip("Kolor linii środkowej (oś).")]
    public Color axisColor = new Color(1f, 0.5f, 0f, 0.2f);
    public float lineThickness = 4f;
    public float axisThickness = 2f;
    [Range(16, 512)] public int samples = 180;
    [Tooltip("Margines wewnętrzny w pikselach.")]
    public float padding = 14f;

    [Header("Mapowanie fali")]
    [Tooltip("Liczba cykli przy MIN częstotliwości (zawsze > 0, żeby oscylacja była widoczna).")]
    public float minCycles = 0.7f;
    [Tooltip("Liczba cykli przy MAX częstotliwości.")]
    public float maxCycles = 2.5f;
    [Range(0f, 1f)]
    [Tooltip("Maksymalna amplituda jako frakcja połowy wysokości.")]
    public float amplitudeFraction = 0.45f;
    [Range(0f, 1f)]
    [Tooltip("Maksymalne pionowe przesunięcie ('długość') jako frakcja połowy wysokości.")]
    public float offsetFraction = 0.40f;

    // raw 0-1: x=amplituda, y=długość, z=częstotliwość
    private Vector3 target = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 current = new Vector3(0.5f, 0.5f, 0.5f);
    private float matchAmount = 0f; // 0 = daleko, 1 = idealne dopasowanie

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false; // wizualizacja nie przechwytuje kliknięć
    }

    /// <summary>Ustawia falę-cel (wartości raw 0-1).</summary>
    public void SetTarget(float ampRaw, float lenRaw, float freqRaw)
    {
        target = new Vector3(ampRaw, lenRaw, freqRaw);
        SetVerticesDirty();
    }

    /// <summary>Ustawia falę aktualną (wartości raw 0-1) — wołane co klatkę.</summary>
    public void SetCurrent(float ampRaw, float lenRaw, float freqRaw)
    {
        current = new Vector3(ampRaw, lenRaw, freqRaw);

        // Jak blisko celu (do barwienia fali na zielono).
        const float vr = 0.18f;
        float c = 0f;
        c += 1f - Mathf.Min(1f, Mathf.Abs(current.x - target.x) / vr);
        c += 1f - Mathf.Min(1f, Mathf.Abs(current.y - target.y) / vr);
        c += 1f - Mathf.Min(1f, Mathf.Abs(current.z - target.z) / vr);
        matchAmount = c / 3f;

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();

        // Oś środkowa
        float left = rect.xMin + padding;
        float right = rect.xMax - padding;
        float midY = rect.center.y;
        AddSegment(vh, new Vector2(left, midY), new Vector2(right, midY), axisColor, axisThickness);

        // Fale: najpierw cel (pod spodem), potem aktualna (na wierzchu).
        // Aktualna zmienia kolor żółty -> zielony w miarę dopasowania.
        DrawWave(vh, rect, target, targetColor);
        float greenT = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, matchAmount));
        Color curCol = Color.Lerp(currentColor, matchedColor, greenT);
        DrawWave(vh, rect, current, curCol);
    }

    void DrawWave(VertexHelper vh, Rect rect, Vector3 raw, Color color)
    {
        float left = rect.xMin + padding;
        float right = rect.xMax - padding;
        float midY = rect.center.y;
        float halfH = rect.height * 0.5f - padding;
        if (halfH < 1f) halfH = 1f;

        // raw -> display 0..10 (jak WaveTweakingMiniGame.ToDisplayValue: raw 0 -> 10, raw 1 -> 0)
        float ampD = WaveTweakingMiniGame.ToDisplayValue(raw.x);
        float lenD = WaveTweakingMiniGame.ToDisplayValue(raw.y);
        float freqD = WaveTweakingMiniGame.ToDisplayValue(raw.z);

        // Trzy NIEZALEŻNE, zawsze widoczne i jednoznaczne cechy (co widać = to się liczy):
        //   amplituda    -> wysokość oscylacji,
        //   częstotliwość-> liczba cykli (zawsze >= minCycles, oscylacja zawsze widoczna),
        //   długość      -> pionowe przesunięcie całej fali (bez fazy = bez okresowej dwuznaczności).
        float amplitude = (ampD / 10f) * (amplitudeFraction * halfH);
        float cycles = Mathf.Lerp(minCycles, maxCycles, freqD / 10f);
        float offset = Mathf.Lerp(-offsetFraction * halfH, offsetFraction * halfH, lenD / 10f);

        int n = Mathf.Max(2, samples);
        Vector2 prev = Vector2.zero;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)(n - 1);
            float x = Mathf.Lerp(left, right, t);
            float y = midY + offset + amplitude * Mathf.Sin(t * cycles * Mathf.PI * 2f);
            Vector2 p = new Vector2(x, y);
            if (i > 0) AddSegment(vh, prev, p, color, lineThickness);
            prev = p;
        }
    }

    void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, Color color, float thickness)
    {
        Vector2 delta = b - a;
        if (delta.sqrMagnitude < 1e-6f) return;
        Vector2 dir = delta.normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int idx = vh.currentVertCount;
        vh.AddVert(a - normal, color, Vector2.zero);
        vh.AddVert(a + normal, color, Vector2.zero);
        vh.AddVert(b + normal, color, Vector2.zero);
        vh.AddVert(b - normal, color, Vector2.zero);
        vh.AddTriangle(idx, idx + 1, idx + 2);
        vh.AddTriangle(idx, idx + 2, idx + 3);
    }
}
