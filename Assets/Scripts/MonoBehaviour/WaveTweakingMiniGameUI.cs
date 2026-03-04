using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WaveTweakingMiniGameUI : MonoBehaviour
{
    [Header("MiniGame Config")]
    private WaveTweakingMiniGame miniGameConfig;

    [Header("UI Sliders")]
    private ConsoleSliderObject amplitudeSlider;
    private ConsoleSliderObject lengthSlider;
    private ConsoleSliderObject frequencySlider;

    [SerializeField] private TextMeshProUGUI requierdValues;
    [SerializeField] private TextMeshProUGUI actualValues;

    private float amplitude;
    private float length;
    private float frequency;
    private bool active = false;

    private float requieredAmplitude;
    private float requieredLength;
    private float requieredFrequency;


    void Update()
    {
        if (!active) return;

        amplitude = amplitudeSlider.GetCurrentValue();
        length = lengthSlider.GetCurrentValue();
        frequency = frequencySlider.GetCurrentValue();
        actualValues.text = $"{amplitude:F2}, {length:F2}, {frequency:F2}";
    }

    public void Setup(WaveTweakingMiniGame config, ConsoleSliderObject amp, ConsoleSliderObject len, ConsoleSliderObject freq)
    {
        miniGameConfig = config;
        active = true;

        requieredAmplitude = miniGameConfig.requiredValues[0];
        requieredLength = miniGameConfig.requiredValues[1];
        requieredFrequency = miniGameConfig.requiredValues[2];

        amplitudeSlider = amp;
        lengthSlider = len;
        frequencySlider = freq;

        actualValues.text = $"{amplitude:F2}, {length:F2}, {frequency:F2}";
        requierdValues.text = $"{requieredAmplitude:F2}, {requieredLength:F2}, {requieredFrequency:F2}";
    }

}