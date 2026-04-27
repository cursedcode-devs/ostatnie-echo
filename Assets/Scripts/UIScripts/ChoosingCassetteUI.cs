using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class ChoosingCassetteUI : MonoBehaviour
{
    public GameObject choosingCassetteCanvas;
    public GameManager gameManager;
    public Airtime airtime;
    private GameObject selectedObject;
    private PlayableObject playableObject;
    //public GameObject[] cassetteSlots;
    public TextMeshProUGUI[] cassetteSlotTexts;

    public void clickedSlot(int slot)
    {
        //LastSelectedObject bo jak klikniesz na lewym przyciskiem myszy na przycisk slotu to kaseta zd¹¿y siê zdeselectowaæ
        selectedObject = gameManager.selectionHandler.GetLastSelectedObject();

        if (selectedObject == null || !selectedObject.CompareTag("Playable"))
        {
            airtime.emptySlot(slot);
            cassetteSlotTexts[slot].text = "Slot " + (slot + 1);
            return;
        }

        if (selectedObject.CompareTag("Playable"))
        {
            playableObject = selectedObject.GetComponent<PlayableObject>();
            airtime.setSlot(playableObject.data, slot);
            cassetteSlotTexts[slot].text = playableObject.data.name + " U¿ycie: " + playableObject.data.GetTimesUsed();
            gameManager.selectionHandler.ResetLastSelectedObject();
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
