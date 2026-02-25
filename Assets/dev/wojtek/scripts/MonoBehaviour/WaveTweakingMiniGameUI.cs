using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveTweakingMiniGameUI : MonoBehaviour
{
    [Header("MiniGame Config")]
    [SerializeField] private WaveTweakingMiniGame miniGameConfig;

    [Header("UI Sliders")]
    [SerializeField] private Slider amplitudeSlider;
    [SerializeField] private Slider lengthSlider;
    [SerializeField] private Slider frequencySlider;

    [SerializeField] private TextMeshProUGUI actualValues;

    private float amplitude;
    private float length;
    private float frequency;
    private bool active;

    private void Start()
    {
        active = true;


        amplitudeSlider.onValueChanged.AddListener((v) => OnSliderChanged());
        lengthSlider.onValueChanged.AddListener((v) => OnSliderChanged());
        frequencySlider.onValueChanged.AddListener((v) => OnSliderChanged());

        amplitudeSlider.value = 0f;
        lengthSlider.value = 0f;
        frequencySlider.value = 0f;
    }

    private void OnSliderChanged()
    {
        if (!active) return;

        amplitude = amplitudeSlider.value;
        length = lengthSlider.value;
        frequency = frequencySlider.value;
        actualValues.text = amplitude.ToString() + ", " + length.ToString() + ", " + frequency.ToString();
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