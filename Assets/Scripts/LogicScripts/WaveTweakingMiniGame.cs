using Unity.VectorGraphics;
using UnityEngine;

[CreateAssetMenu(menuName = "MiniGames/Wave Tweaking")]
public class WaveTweakingMiniGame : MiniGame
{
    [Header("Required Values")]
    [SerializeField] public Vector3 requiredValues = new Vector3(2f, 2f, 2f);

    [SerializeField] public float amplitude;
    [SerializeField] public float length;
    [SerializeField] public float frequency;
    [SerializeField] private bool active;
    [SerializeField] private GameObject WaveTweakingUI;
    GameObject uiInstance;

    public override void Play()
    {
        amplitude = 0f;
        length = 0f;
        frequency = 0f;
        active = true;
        uiInstance = Instantiate(WaveTweakingUI);


        Debug.Log("Starting Wave Tweaking MiniGame");
    }

    public override void Stop()
    {
        active = false;
        Destroy(uiInstance);

        Debug.Log("Stopping Wave Tweaking MiniGame");
    }

}