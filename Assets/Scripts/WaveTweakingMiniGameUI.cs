using TMPro;
using UnityEngine;

/// <summary>
/// WaveTweakingMiniGameUI
/// Displays slider values remapped from raw 0.49-0.51 band to 10-0 scale.
/// Built and wired by WaveTweakingSceneBuilder.
/// </summary>
public class WaveTweakingMiniGameUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI ampValueText;
    [SerializeField] public TextMeshProUGUI lenValueText;
    [SerializeField] public TextMeshProUGUI freqValueText;
    [SerializeField] public TextMeshProUGUI ampTargetText;
    [SerializeField] public TextMeshProUGUI lenTargetText;
    [SerializeField] public TextMeshProUGUI freqTargetText;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public WaveTweakingGraph waveGraph;

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

        if (ampValueText != null) ampValueText.text = "5.0";
        if (lenValueText != null) lenValueText.text = "5.0";
        if (freqValueText != null) freqValueText.text = "5.0";

        if (ampTargetText != null) ampTargetText.text = WaveTweakingMiniGame.ToDisplayValue(requiredAmplitude).ToString("F1");
        if (lenTargetText != null) lenTargetText.text = WaveTweakingMiniGame.ToDisplayValue(requiredLength).ToString("F1");
        if (freqTargetText != null) freqTargetText.text = WaveTweakingMiniGame.ToDisplayValue(requiredFrequency).ToString("F1");

        if (statusText != null)
            statusText.text = "Użyj suwaków poniżej, aby dopasować obie fale sygnału.";

        if (waveGraph != null)
            waveGraph.SetTarget(requiredAmplitude, requiredLength, requiredFrequency);
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

        float a = amplitudeSlider.GetCurrentValue();
        float l = lengthSlider.GetCurrentValue();
        float f = frequencySlider.GetCurrentValue();

        if (ampValueText != null) ampValueText.text = WaveTweakingMiniGame.ToDisplayValue(a).ToString("F1");
        if (lenValueText != null) lenValueText.text = WaveTweakingMiniGame.ToDisplayValue(l).ToString("F1");
        if (freqValueText != null) freqValueText.text = WaveTweakingMiniGame.ToDisplayValue(f).ToString("F1");

        if (waveGraph != null)
            waveGraph.SetCurrent(a, l, f);
    }
}