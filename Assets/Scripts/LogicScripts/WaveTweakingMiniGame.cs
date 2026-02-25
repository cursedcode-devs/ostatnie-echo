using Unity.VectorGraphics;
using UnityEngine;

//[CreateAssetMenu(menuName = "MiniGames/Wave Tweaking")]
[System.Serializable]
public class WaveTweakingMiniGame : MiniGame
{
    [Header("Required Values")]
    [SerializeField] public Vector3 requiredValues = new Vector3(2f, 2f, 2f);

    [SerializeField] public float amplitude;
    [SerializeField] public float length;
    [SerializeField] public float frequency;
    [SerializeField] private bool active;
    [SerializeField] private GameObject WaveTweakingUI;
    public GameObject uiInstance;

    public WaveTweakingMiniGame()
    {
        amplitude = 0f;
        length = 0f;
        frequency = 0f;
        active = false;
    }

    public override void Play()
    {
        amplitude = 0f;
        length = 0f;
        frequency = 0f;
        active = true;
        uiInstance = GameObject.Instantiate(WaveTweakingUI);


        WaveTweakingMiniGameUI uiScript = uiInstance.GetComponent<WaveTweakingMiniGameUI>();
        if (uiScript != null)
        {
            uiScript.Setup(this);
        }

        Debug.Log("Starting Wave Tweaking MiniGame");
    }

    public override void Stop()
    {
        active = false;
        GameObject.Destroy(uiInstance);

        Debug.Log("Stopping Wave Tweaking MiniGame");
    }

}