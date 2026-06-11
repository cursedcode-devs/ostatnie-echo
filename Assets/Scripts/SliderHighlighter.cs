using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// SliderHighlighter — sygnalizuje interaktywność suwaków minigry delikatną
/// pomarańczową poświatą (jak przedmioty, na które można najechać).
/// Pasywnie świeci bardzo subtelnie, po najechaniu myszką mocniej.
/// Nakłada półprzezroczysty pomarańczowy materiał na renderery (jak SimpleGlowOutline),
/// a intensywność reguluje przez alpha. Włączany/wyłączany przez adapter minigry.
/// </summary>
[DisallowMultipleComponent]
public class SliderHighlighter : MonoBehaviour
{
    [Tooltip("Kolor poświaty (pomarańczowy, jak hover innych przedmiotów).")]
    public Color glowColor = new Color(1f, 0.5f, 0f, 1f);
    [Range(0f, 1f)]
    [Tooltip("Intensywność pasywna — bardzo delikatna.")]
    public float passiveIntensity = 0.12f;
    [Range(0f, 1f)]
    [Tooltip("Intensywność po najechaniu — mocniejsza.")]
    public float hoverIntensity = 0.5f;
    [Tooltip("Szybkość płynnego przejścia między stanami.")]
    public float fadeSpeed = 8f;

    private Renderer[] rends;
    private Material glowMat;
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Camera cam;
    private float currentIntensity = 0f;
    private bool built = false;

    void Build()
    {
        rends = GetComponentsInChildren<Renderer>();

        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Unlit/Color");
        glowMat = new Material(sh);

        // Przezroczysty (alpha blend), bez zapisu do bufora głębi — czysta poświata.
        glowMat.SetFloat("_Surface", 1f);
        glowMat.SetFloat("_Blend", 0f);
        glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glowMat.SetInt("_ZWrite", 0);
        glowMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        glowMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        built = true;
    }

    void OnEnable()
    {
        if (!built) Build();
        if (cam == null) cam = Camera.main;
        currentIntensity = 0f;
        AddGlow();
        ApplyIntensity(0f);
    }

    void OnDisable()
    {
        RemoveGlow();
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        float target = IsHovered() ? hoverIntensity : passiveIntensity;
        currentIntensity = Mathf.MoveTowards(currentIntensity, target, fadeSpeed * Time.unscaledDeltaTime);
        ApplyIntensity(currentIntensity);
    }

    void AddGlow()
    {
        if (rends == null) return;
        foreach (var r in rends)
        {
            if (r == null) continue;
            if (!originalMaterials.ContainsKey(r))
                originalMaterials[r] = r.materials;

            var cur = r.materials;
            var nw = new Material[cur.Length + 1];
            for (int i = 0; i < cur.Length; i++) nw[i] = cur[i];
            nw[nw.Length - 1] = glowMat;
            r.materials = nw;
        }
    }

    void RemoveGlow()
    {
        if (rends == null) return;
        foreach (var r in rends)
        {
            if (r == null) continue;
            if (originalMaterials.TryGetValue(r, out var orig))
                r.materials = orig;
        }
    }

    void ApplyIntensity(float intensity)
    {
        if (glowMat == null) return;
        Color c = glowColor;
        c.a = intensity;
        if (glowMat.HasProperty("_BaseColor")) glowMat.SetColor("_BaseColor", c);
        if (glowMat.HasProperty("_Color")) glowMat.SetColor("_Color", c);
    }

    bool IsHovered()
    {
        if (cam == null) return false;
        var mouse = Mouse.current;
        if (mouse == null) return false;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return hit.collider != null && hit.collider.GetComponentInParent<SliderHighlighter>() == this;

        return false;
    }
}
