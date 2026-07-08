using UnityEngine;
using UnityEngine.UI;
using System.Collections;


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

    void Awake()
    {
        glowImage = GetComponent<Image>();
        intensity = 0f;
        SetActive(false);
    }

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
            intensity = 0f;
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {

        pulseCoroutine = null;
    }

    IEnumerator PulseGlow()
    {
        while (true)
        {
            float t = Time.unscaledTime * pulseSpeed;
            float sin = (Mathf.Sin(t) + 1f) * 0.5f; // 0..1

            float targetIntensity = isActive ? 1f : 0f;
            intensity = Mathf.MoveTowards(intensity, targetIntensity, Time.unscaledDeltaTime * 10f);

            if (glowImage)
            {
                Color c = glowImage.color;
                c.a = Mathf.Lerp(minAlpha, maxAlpha, sin) * intensity;
                glowImage.color = c;
            }

            float baseScale = Mathf.Lerp(minScale, maxScale, sin);
            float s = Mathf.Lerp(minScale, baseScale, intensity);
            transform.localScale = Vector3.one * s;

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
