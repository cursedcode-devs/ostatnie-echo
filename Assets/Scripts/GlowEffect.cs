using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// GlowEffect - Efekt świecenia dla świateł
/// ==========================================
/// Dodaj do child-obiektu przycisku-światła (np. "Glow").
/// Tworzy animowaną poświatę gdy światło jest włączone.
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
    private float intensity = 0f;

    // -------------------------------------------------------
    void Awake()
    {
        glowImage = GetComponent<Image>();
        intensity = 0f;
        SetActive(false);
    }

    // -------------------------------------------------------
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (active)
        {
            gameObject.SetActive(true);
            if (pulseCoroutine == null && gameObject.activeInHierarchy)
            {
                pulseCoroutine = StartCoroutine(PulseGlow());
            }
        }
        else
        {
            // Jeśli coroutine nie działa (np. zatrzymana przez dezaktywację rodzica),
            // natychmiast ukrywamy obiekt i zerujemy intensywność.
            if (pulseCoroutine == null || !gameObject.activeInHierarchy)
            {
                intensity = 0f;
                gameObject.SetActive(false);
                pulseCoroutine = null;
            }
        }
    }

    void OnEnable()
    {
        if (isActive)
        {
            if (pulseCoroutine == null)
            {
                pulseCoroutine = StartCoroutine(PulseGlow());
            }
        }
        else
        {
            // Zabezpieczenie: jeśli aktywowano rodzica, a ten obiekt ma być wyłączony, ukryj go.
            intensity = 0f;
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        // Kiedy Unity wyłącza obiekt (np. zamykanie minigry), coroutine automatycznie umiera.
        // Czyścimy referencję, żeby uniknąć martwych coroutinów przy kolejnym uruchomieniu.
        pulseCoroutine = null;
    }

    // -------------------------------------------------------
    IEnumerator PulseGlow()
    {
        while (true)
        {
            float t = Time.unscaledTime * pulseSpeed;
            float sin = (Mathf.Sin(t) + 1f) * 0.5f; // 0..1

            // Płynne włączanie/wyłączanie (fade in/out), zapobiega gwałtownym skokom "pop-in"
            float targetIntensity = isActive ? 1f : 0f;
            intensity = Mathf.MoveTowards(intensity, targetIntensity, Time.unscaledDeltaTime * 10f);

            // Alpha
            if (glowImage)
            {
                Color c = glowImage.color;
                c.a = Mathf.Lerp(minAlpha, maxAlpha, sin) * intensity;
                glowImage.color = c;
            }

            // Skala
            float baseScale = Mathf.Lerp(minScale, maxScale, sin);
            float s = Mathf.Lerp(minScale, baseScale, intensity);
            transform.localScale = Vector3.one * s;

            // Zakończ, gdy całkowicie zgaśnie
            if (!isActive && intensity <= 0f)
            {
                gameObject.SetActive(false);
                pulseCoroutine = null;
                yield break;
            }

            yield return null;
        }
    }
}
