using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveTweakingMiniGameUI : MonoBehaviour
{
    [Header("MiniGame Config")]
    private WaveTweakingMiniGame miniGameConfig;

    [Header("UI Sliders")]
    [SerializeField] private ConsoleSliderObject amplitudeSlider;
    [SerializeField] private ConsoleSliderObject lengthSlider;
    [SerializeField] private ConsoleSliderObject frequencySlider;

    [SerializeField] private TextMeshProUGUI actualValues;

    private float amplitude;
    private float length;
    private float frequency;
    private bool active;

    private void OnEnable()
    {
        active = true;

        amplitudeSlider.onValueChanged.AddListener(OnSliderChanged);
        lengthSlider.onValueChanged.AddListener(OnSliderChanged);
        frequencySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    public void Setup(WaveTweakingMiniGame config)
    {
        miniGameConfig = config;
    }

    private void OnSliderChanged(float value)
    {
        if (!active) return;

        amplitude = amplitudeSlider.GetCurrentValue();
        length = lengthSlider.GetCurrentValue();
        frequency = frequencySlider.GetCurrentValue();

        actualValues.text = $"{amplitude:F2}, {length:F2}, {frequency:F2}";

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {

        if (Mathf.Abs(amplitude - miniGameConfig.requiredValues.x) < 0.01f &&
            Mathf.Abs(length - miniGameConfig.requiredValues.y) < 0.01f &&
            Mathf.Abs(frequency - miniGameConfig.requiredValues.z) < 0.01f)
        {
            active = false;
            miniGameConfig.Stop();
        }
    }
}