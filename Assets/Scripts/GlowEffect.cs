using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// GlowEffect - Efekt świecenia dla świateł
/// ==========================================
/// Dodaj do child-obiektu przycisku-światła (np. "Glow").
/// Tworzy animowaną poświatę gdy światło jest włączone.
/// 
/// SETUP:
/// Light_00 (Button + Image [ciemne])
///   ├── LightBulb (Image - żółte koło)
///   └── Glow (Image + GlowEffect - biały sprite z alpha, skalowany)
/// </summary>
public class GlowEffect : MonoBehaviour
{
    [Header("Ustawienia blasku")]
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.9f;
    public float pulseSpeed = 2f;

    [Header("Skalowanie")]
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private Image glowImage;
    private bool isActive = false;
    private Coroutine pulseCoroutine;

    // -------------------------------------------------------
    void Awake()
    {
        glowImage = GetComponent<Image>();
        SetActive(false);
    }

    // -------------------------------------------------------
    public void SetActive(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);

        if (active)
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseGlow());
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
        }
    }

    // -------------------------------------------------------
    IEnumerator PulseGlow()
    {
        float t = Random.Range(0f, Mathf.PI * 2f); // losowy start fazy
        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float sin = (Mathf.Sin(t) + 1f) * 0.5f; // 0..1

            // Alpha
            if (glowImage)
            {
                Color c = glowImage.color;
                c.a = Mathf.Lerp(minAlpha, maxAlpha, sin);
                glowImage.color = c;
            }

            // Skala
            float s = Mathf.Lerp(minScale, maxScale, sin);
            transform.localScale = Vector3.one * s;

            yield return null;
        }
    }
}
