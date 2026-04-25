using TMPro;
using UnityEngine;

public class ChoosingCassetteUI : MonoBehaviour
{
    public GameObject choosingCassetteCanvas;
    public GameManager gameManager;
    public Airtime airtime;
    private PlayableObject selectedObject;
    //public GameObject[] cassetteSlots;
    public TextMeshProUGUI[] cassetteSlotTexts;

    public void clickedSlot(int slot)
    {
        //LastSelectedObject bo jak klikniesz na lewym na przycisk to kaseta zd¹¿y siê zdeselectowaæ
        if (gameManager.selectionHandler.GetLastSelectedObject() == null)
        {
            airtime.emptySlot(slot);
            cassetteSlotTexts[slot].text = "Slot " + slot;
            return;
        }

        selectedObject = gameManager.selectionHandler.GetLastSelectedObject().GetComponent<PlayableObject>();
        if (selectedObject.CompareTag("Playable"))
        {
            airtime.setSlot(selectedObject.data, slot);
            cassetteSlotTexts[slot].text = selectedObject.data.name;
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
