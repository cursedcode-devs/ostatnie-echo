using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI hipHopText;
    public TextMeshProUGUI rockText;
    public TextMeshProUGUI popText;
    public TextMeshProUGUI discoText;

    private RadioStation radioStation;

    public void Initialize(RadioStation rs)
    {
        radioStation = rs;
    }

    void Update()
    {
        if (radioStation == null) return;
        UpdateUI();
    }

    public void UpdateUI()
    {
        moneyText.text  = "Money: "  + radioStation.GetCurrentMoney().ToString("F2") + "$";
        hipHopText.text = "HipHop: " + radioStation.currentListeners.hipHop;
        rockText.text   = "Rock: "   + radioStation.currentListeners.rock;
        popText.text  = "Pop: "  + radioStation.currentListeners.pop;
        discoText.text  = "Disco: "  + radioStation.currentListeners.disco;
    }
}