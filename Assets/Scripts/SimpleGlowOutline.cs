using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Prosty skrypt podświetlenia, który można podpiąć pod obiekt.
/// Po jego włączeniu (np. z poziomu ZoomHandler) podmienia/dodaje pomarańczowy materiał.
/// Działa najlepiej z prostymi obiektami.
/// </summary>
public class SimpleGlowOutline : MonoBehaviour
{
    [Tooltip("Kolor podświetlenia (domyślnie pomarańczowy).")]
    public Color glowColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
    
    [Tooltip("Renderery, które mają być podświetlane. Zostaw puste by znaleźć automatycznie.")]
    public Renderer[] targetRenderers;

    [Tooltip("Gotowy materiał przezroczysty z Resources.")]
    public Material glowMaterial;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }

        if (glowMaterial == null)
        {
            glowMaterial = Resources.Load<Material>("shadermaterial");
        }
    }

    void OnEnable()
    {
        // Kiedy skrypt jest włączany, dodajemy materiał podświetlenia jako ostatni element w tablicy materiałów
        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;

            if (!originalMaterials.ContainsKey(rend))
            {
                originalMaterials[rend] = rend.materials;
            }

            Material[] currentMats = rend.materials;
            Material[] newMats = new Material[currentMats.Length + 1];
            
            for (int i = 0; i < currentMats.Length; i++)
                newMats[i] = currentMats[i];
                
            newMats[newMats.Length - 1] = glowMaterial;
            rend.materials = newMats;
        }
    }

    void OnDisable()
    {
        // Przywracamy oryginalne materiały
        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;
            
            if (originalMaterials.ContainsKey(rend))
            {
                rend.materials = originalMaterials[rend];
            }
        }
    }
}
