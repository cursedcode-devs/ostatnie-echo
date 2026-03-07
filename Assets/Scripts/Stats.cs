using UnityEngine;
using TMPro;
public class NewMonoBehaviourScript : MonoBehaviour
{
    public RadioStation radioStation;

    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI hipHopText;
    public TextMeshProUGUI rockText;
    public TextMeshProUGUI metalText;
    public TextMeshProUGUI discoText;

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }
    private void UpdateUI()
    {
        moneyText.text = "Money: " + radioStation.GetCurrentMoney().ToString("F2") + "$";

        hipHopText.text = "HipHop: " + radioStation.currentListeners.hipHop;
        rockText.text = "Rock: " + radioStation.currentListeners.rock;
        metalText.text = "Metal: " + radioStation.currentListeners.metal;
        discoText.text = "Disco: " + radioStation.currentListeners.disco;
    }
}
