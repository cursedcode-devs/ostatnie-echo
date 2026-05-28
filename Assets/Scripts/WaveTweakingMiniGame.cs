using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// WaveTweakingMiniGame — core logic only.
/// Does NOT inherit BaseMiniGame — it's a plain MonoBehaviour like LightsOutMiniGame.
/// WaveTweakingMiniGameAdapter wraps it into the BaseMiniGame system,
/// exactly like LightsOutMiniGameAdapter wraps LightsOutMiniGame.
///
/// Sliders physically move 0-1. Only the band 0.49-0.51 is used.
/// That band maps to display values 10 (at 0.49) → 0 (at 0.51).
/// </summary>
public class WaveTweakingMiniGame : MonoBehaviour
{
    public const float BandMin = 0f;
    public const float BandMax = 1f;

    [Header("Win tolerance (raw 0-1 units)")]
    [Tooltip("0.0001 ≈ 0.01 on the displayed 10-0 scale.")]
    public float tolerance = 0.01f;

    [Header("References (set by WaveTweakingSceneBuilder)")]
    public ConsoleSliderObject amplitudeSlider;
    public ConsoleSliderObject lengthSlider;
    public ConsoleSliderObject frequencySlider;
    public WaveTweakingMiniGameUI uiScript;

    // Fired when player wins — WaveTweakingMiniGameAdapter listens to this
    public event System.Action OnWin;

    private float[] requiredValues = new float[3];
    private bool active = false;
    private bool won = false;
    private ConsoleSliderObject activeSlider = null;

    // ------------------------------------------------------------------
    public void StartGame()
    {
        won = false;
        active = true;

        requiredValues[0] = Random.Range(BandMin, BandMax);
        requiredValues[1] = Random.Range(BandMin, BandMax);
        requiredValues[2] = Random.Range(BandMin, BandMax);

        if (uiScript != null)
            uiScript.Setup(requiredValues, amplitudeSlider, lengthSlider, frequencySlider);

        Debug.Log("[WaveTweakingMiniGame] Game started.");
    }

    // ------------------------------------------------------------------
    void Update()
    {
        if (!active || won) return;
        HandleSliderInput();
        CheckWin();
    }

    private void HandleSliderInput()
    {
        if(Camera.main==null)
            return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ConsoleSliderObject hitSlider = hit.transform.GetComponent<ConsoleSliderObject>();
                if (hitSlider == amplitudeSlider || hitSlider == lengthSlider || hitSlider == frequencySlider)
                {
                    activeSlider = hitSlider;
                    activeSlider.OnMouseClick();
                }
            }
        }

        if (Mouse.current.leftButton.isPressed && activeSlider != null)
            activeSlider.OnMousePressed();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            activeSlider = null;
    }

    private void CheckWin()
    {
        if (amplitudeSlider == null || lengthSlider == null || frequencySlider == null) return;

        bool allMatch = Mathf.Abs(amplitudeSlider.GetCurrentValue() - requiredValues[0]) < tolerance
                     && Mathf.Abs(lengthSlider.GetCurrentValue()    - requiredValues[1]) < tolerance
                     && Mathf.Abs(frequencySlider.GetCurrentValue() - requiredValues[2]) < tolerance;

        if (allMatch)
            StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        won = true;
        active = false;

        if (uiScript != null)
            uiScript.ShowWin();

        Debug.Log("[WaveTweakingMiniGame] Win!");

        // Small flash delay like LightsOut, then fire event
        yield return new WaitForSeconds(0.5f);
        OnWin?.Invoke();
    }

    // ------------------------------------------------------------------
    /// <summary>Maps raw 0-1 slider value → displayed 10-0 scale.</summary>
    public static float ToDisplayValue(float raw)
    {
        float t = Mathf.InverseLerp(BandMin, BandMax, raw);
        return Mathf.Lerp(10f, 0f, t);
    }
}