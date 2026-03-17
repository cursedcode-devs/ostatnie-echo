using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ConsoleSlider", menuName = "Radio/ConsoleSlider")]
public class ConsoleSlider : ScriptableObject
{
    [SerializeField] private float sliderValue;

    [SerializeField] private SliderType sliderType;

    public enum SliderType
    {
            LengthSlider,
            ApmlitudeSlider,
            FrequencySlider
    }


}
