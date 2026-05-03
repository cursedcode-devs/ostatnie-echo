using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.Rendering.DebugUI;

public class ChoosingCassetteUI : MonoBehaviour
{
    public GameObject choosingCassetteCanvas;
    public GameManager gameManager;
    public GameObject cassettePlayer;
    public Airtime airtime;
    private GameObject selectedObject;
    private PlayableObject playableObject;
    //public GameObject[] cassetteSlots;
    public TextMeshProUGUI[] cassetteSlotTexts;
    public TextMeshProUGUI[] StatTexts;
    private bool active = false;

    public void clickedSlot(int slot)
    {
        //LastSelectedObject bo jak klikniesz na lewym przyciskiem myszy na przycisk slotu to kaseta zd¹¿y siê zdeselectowaæ
        selectedObject = gameManager.selectionHandler.GetLastSelectedObject();

        if (selectedObject == null || !selectedObject.CompareTag("Playable"))
        {
            airtime.emptySlot(slot);
            cassetteSlotTexts[slot].text = "Slot " + (slot + 1);
            StatTexts[slot].text = "";
            StatTexts[slot].text = "HipHop: \nRock: \nMetal: \nDisco: ";
            StatTexts[3].text = "HipHop: " + airtime.GetStatsSum(0) + " " + airtime.GetStatsSum(0, CassetteTypes.Ad) + "z³\nRock: "
                + airtime.GetStatsSum(1) + " " + airtime.GetStatsSum(1, CassetteTypes.Ad) + "z³\nMetal: "
                + airtime.GetStatsSum(2) + " " + airtime.GetStatsSum(2, CassetteTypes.Ad) + "z³\nDisco: "
                + airtime.GetStatsSum(3) + " " + airtime.GetStatsSum(3, CassetteTypes.Ad) + "z³";
            PlayCassetteSound();
            return;
        }

        if (selectedObject.CompareTag("Playable"))
        {
            playableObject = selectedObject.GetComponent<PlayableObject>();
            airtime.setSlot(playableObject.data, slot);
            cassetteSlotTexts[slot].text = playableObject.data.GetName() + " U¿ycie: " + playableObject.data.GetTimesUsed();
            GenreValues values = playableObject.data.GetCassetteValues();

            if (playableObject.data.GetType() == CassetteTypes.Ad)
            {
                StatTexts[slot].text = "HipHop: " + values.hipHop / 100f + "z³" + "\nRock: " + values.rock / 100f + "z³" + "\nMetal: " + values.metal / 100f + "z³" + "\nDisco: " + values.disco / 100f + "z³";
            }
            else if (playableObject.data.GetType() == CassetteTypes.Music)
            {
                StatTexts[slot].text = "HipHop: " + values.hipHop / 100f + "\nRock: " + values.rock / 100f + "\nMetal: " + values.metal / 100f + "\nDisco: " + values.disco / 100f;
            }

            gameManager.selectionHandler.ResetLastSelectedObject();
            StatTexts[3].text = "HipHop: " + airtime.GetStatsSum(0) + " " + airtime.GetStatsSum(0, CassetteTypes.Ad) + "z³\nRock: "
                + airtime.GetStatsSum(1) + " " + airtime.GetStatsSum(1, CassetteTypes.Ad) + "z³\nMetal: "
                + airtime.GetStatsSum(2) + " " + airtime.GetStatsSum(2, CassetteTypes.Ad) + "z³\nDisco: "
                + airtime.GetStatsSum(3) + " " + airtime.GetStatsSum(3, CassetteTypes.Ad) + "z³";
            PlayCassetteSound();
        }
    }

    private void PlayCassetteSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(gameManager.putCasetteInSound, cassettePlayer.transform.position);
    }

    public void ToggleVisibility(bool isPlaying)
    {
        if (isPlaying)
            return;
        choosingCassetteCanvas.SetActive(!active);
        active = !active;
    }

    public void Show()
    {
        choosingCassetteCanvas.SetActive(true);
    }

    public void Hide()
    {
        choosingCassetteCanvas.SetActive(false);
    }

    public void ResetSlotText()
    {
        for (int i = 0; i < cassetteSlotTexts.Length; i++)
        {
            cassetteSlotTexts[i].text = "Slot " + (i + 1);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
