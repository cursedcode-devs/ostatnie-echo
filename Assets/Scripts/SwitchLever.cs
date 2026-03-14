using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// SwitchLever - Wizualny przełącznik dźwigniowy
/// ================================================
/// Dodaj ten skrypt do każdego przycisku-przełącznika.
/// Animuje "dźwignię" (child RectTransform) góra/dół przy kliknięciu.
/// 
/// SETUP:
/// Switch_00 (Button + SwitchLever)
///   └── Lever (Image - pionowy prostokąt, punkt obrotu = dół)
///         └── Knob (Image - kulka na końcu dźwigni)
/// </summary>
[RequireComponent(typeof(Button))]
public class SwitchLever : MonoBehaviour
{
    [Header("Dźwignia")]
    [Tooltip("Transform dźwigni do animacji (child obiektu)")]
    public RectTransform leverTransform;

    [Tooltip("Kąt w pozycji 'wyłączone' (np. +30 stopni)")]
    public float angleOff = 30f;

    [Tooltip("Kąt w pozycji 'włączone' (np. -30 stopni)")]
    public float angleOn = -30f;

    [Tooltip("Przełącznik wraca na pozycję 'wyłączone' po animacji")]
    public bool returnToOff = true;

    [Tooltip("Czas animacji przejścia")]
    public float animTime = 0.15f;

    [Header("LED Indicator")]
    [Tooltip("Mała lampka na przełączniku (opcjonalna)")]
    public Image ledIndicator;
    public Color ledOnColor  = new Color(0.2f, 1f, 0.2f);
    public Color ledOffColor = new Color(0.1f, 0.25f, 0.1f);

    [Header("Dźwięk")]
    public AudioClip clickSound;

    private bool currentState = false;
    private Coroutine animCoroutine;
    private AudioSource audioSource;

    // -------------------------------------------------------
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Ustaw pozycję startową dźwigni
        if (leverTransform)
            leverTransform.localRotation = Quaternion.Euler(0, 0, angleOff);
    }

    // -------------------------------------------------------
    /// <summary>Wywołaj tę metodę zamiast standardowego onClick jeśli chcesz kontrolować stan.</summary>
    public void TriggerSwitch()
    {
        if (returnToOff)
            AnimateAndReturn();
        else
            AnimateToggle();
    }

    // -------------------------------------------------------
    /// <summary>Animuje włączenie i powrót - dla przełączników "momentary".</summary>
    void AnimateAndReturn()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(DoAnimateReturn());
    }

    IEnumerator DoAnimateReturn()
    {
        PlayClick();

        // Idź do ON
        yield return StartCoroutine(RotateLever(angleOff, angleOn, animTime * 0.5f));
        SetLED(true);

        yield return new WaitForSeconds(animTime * 0.3f);

        // Wróć do OFF
        yield return StartCoroutine(RotateLever(angleOn, angleOff, animTime * 0.5f));
        SetLED(false);
    }

    // -------------------------------------------------------
    /// <summary>Animuje toggle (zmiana stanu) dla przełączników trwałych.</summary>
    void AnimateToggle()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(DoAnimateToggle());
    }

    IEnumerator DoAnimateToggle()
    {
        PlayClick();
        currentState = !currentState;
        float from = currentState ? angleOff : angleOn;
        float to   = currentState ? angleOn  : angleOff;
        yield return StartCoroutine(RotateLever(from, to, animTime));
        SetLED(currentState);
    }

    // -------------------------------------------------------
    IEnumerator RotateLever(float fromAngle, float toAngle, float duration)
    {
        if (!leverTransform) yield break;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0, 1, t / duration);
            float angle = Mathf.Lerp(fromAngle, toAngle, p);
            leverTransform.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        leverTransform.localRotation = Quaternion.Euler(0, 0, toAngle);
    }

    // -------------------------------------------------------
    void SetLED(bool on)
    {
        if (ledIndicator)
            ledIndicator.color = on ? ledOnColor : ledOffColor;
    }

    void PlayClick()
    {
        if (clickSound && audioSource)
            audioSource.PlayOneShot(clickSound, 0.8f);
    }

    // -------------------------------------------------------
    /// <summary>Resetuje wizualny stan przełącznika.</summary>
    public void ResetVisual()
    {
        currentState = false;
        if (leverTransform)
            leverTransform.localRotation = Quaternion.Euler(0, 0, angleOff);
        SetLED(false);
        if (animCoroutine != null) StopCoroutine(animCoroutine);
    }
}
