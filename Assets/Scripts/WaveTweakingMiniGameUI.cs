using TMPro;
using UnityEngine;

/// <summary>
/// WaveTweakingMiniGameUI
/// Displays slider values remapped from raw 0.49-0.51 band to 10-0 scale.
/// Built and wired by WaveTweakingSceneBuilder.
/// </summary>
public class WaveTweakingMiniGameUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI requiredValuesText;
    [SerializeField] public TextMeshProUGUI actualValuesText;
    [SerializeField] public TextMeshProUGUI statusText;

    private ConsoleSliderObject amplitudeSlider;
    private ConsoleSliderObject lengthSlider;
    private ConsoleSliderObject frequencySlider;

    private float requiredAmplitude;
    private float requiredLength;
    private float requiredFrequency;

    private bool active = false;

    // ------------------------------------------------------------------
    public void Setup(float[] requiredValues, ConsoleSliderObject amp, ConsoleSliderObject len, ConsoleSliderObject freq)
    {
        requiredAmplitude = requiredValues[0];
        requiredLength    = requiredValues[1];
        requiredFrequency = requiredValues[2];

        amplitudeSlider = amp;
        lengthSlider    = len;
        frequencySlider = freq;

        active = true;

        if (requiredValuesText != null)
            requiredValuesText.text = FormatValues(requiredAmplitude, requiredLength, requiredFrequency);

        if (actualValuesText != null)
            actualValuesText.text = "5.0, 5.0, 5.0";

        if (statusText != null)
            statusText.text = "DOPASUJ FALE";
    }

    public void ShowWin()
    {
        active = false;
        if (statusText != null)
            statusText.text = "SYGNAŁ STABILNY!";
    }

    public void Deactivate() => active = false;

    // ------------------------------------------------------------------
    void Update()
    {
        if (!active) return;
        if (amplitudeSlider == null || lengthSlider == null || frequencySlider == null) return;

        if (actualValuesText != null)
            actualValuesText.text = FormatValues(
                amplitudeSlider.GetCurrentValue(),
                lengthSlider.GetCurrentValue(),
                frequencySlider.GetCurrentValue());
    }

    private string FormatValues(float a, float b, float c)
    {
        return $"{WaveTweakingMiniGame.ToDisplayValue(a):F1}, " +
               $"{WaveTweakingMiniGame.ToDisplayValue(b):F1}, " +
               $"{WaveTweakingMiniGame.ToDisplayValue(c):F1}";
    }
}