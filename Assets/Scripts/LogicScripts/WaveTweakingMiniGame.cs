using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

//[CreateAssetMenu(menuName = "MiniGames/Wave Tweaking")]
[System.Serializable]
public class WaveTweakingMiniGame : MiniGame
{
    [Header("Required Values")]
    [SerializeField] public float[] requiredValues = { 1f, 1f, 1f };

    [SerializeField] public float amplitude;
    [SerializeField] public float length;
    [SerializeField] public float frequency;
    [SerializeField] private bool active;
    [SerializeField] private GameObject WaveTweakingUI;
    private GameObject uiInstance;

    private ConsoleSliderObject amplitudeSlider;
    private ConsoleSliderObject lengthSlider;
    private ConsoleSliderObject frequencySlider;

    public WaveTweakingMiniGame(ConsoleSliderObject amplitudeSlider, ConsoleSliderObject lengthSlider, ConsoleSliderObject frequencySlider)
    {
        amplitude = 0f;
        length = 0f;
        frequency = 0f;
        active = false;

        this.amplitudeSlider = amplitudeSlider;
        this.lengthSlider = lengthSlider;
        this.frequencySlider = frequencySlider;
    }



    public WaveTweakingMiniGame(ConsoleSliderObject amplitudeSlider, ConsoleSliderObject lengthSlider, ConsoleSliderObject frequencySlider, GameObject WaveTweakingUI)
    {
        amplitude = 0f;
        length = 0f;
        frequency = 0f;
        active = false;

        this.amplitudeSlider = amplitudeSlider;
        this.lengthSlider = lengthSlider;
        this.frequencySlider = frequencySlider;
        this.WaveTweakingUI = WaveTweakingUI;
    }

    public override void Start()
    {
        requiredValues = new float[3];
        requiredValues[0] = Random.Range(0.0f, 1.0f);
        requiredValues[1] = Random.Range(0.0f, 1.0f);
        requiredValues[2] = Random.Range(0.0f, 1.0f);
        amplitude = 0f;
        length = 0f;
        frequency = 0f;
        active = true;
        uiInstance = GameObject.Instantiate(WaveTweakingUI);


        WaveTweakingMiniGameUI uiScript = uiInstance.GetComponent<WaveTweakingMiniGameUI>();
        if (uiScript != null)
        {
            uiScript.Setup(this, amplitudeSlider, lengthSlider, frequencySlider);
        }

        Debug.Log("Starting Wave Tweaking MiniGame - WaveTweakingMiniGame.cs");
    }

    public override bool CheckWinCondition()
    {

        if (Mathf.Abs(amplitudeSlider.GetCurrentValue() - requiredValues[0]) < 0.01f &&
            Mathf.Abs(lengthSlider.GetCurrentValue() - requiredValues[1]) < 0.01f &&
            Mathf.Abs(frequencySlider.GetCurrentValue() - requiredValues[2]) < 0.01f)
        {
            active = false;
            Stop();
            return true;
        }

        return false;
    }

    public override void Stop()
    {
        active = false;
        GameObject.Destroy(uiInstance);

        Debug.Log("Stopping Wave Tweaking MiniGame - WaveTweakingMiniGame.cs");
    }

    public override void AddModifier(RadioStation radioStation)
    {
        float newModifier = 0.2f;
        radioStation.AddHourlyListenersModifier(newModifier, newModifier, newModifier, newModifier);
    }

    public GameObject getUiInstance()
    {
        return uiInstance;
    }

}