using TMPro;
using UnityEngine;
using UnityEngine.Playables;

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
    private bool active = false;

    public void clickedSlot(int slot)
    {
        //LastSelectedObject bo jak klikniesz na lewym przyciskiem myszy na przycisk slotu to kaseta zd¹¿y siê zdeselectowaæ
        selectedObject = gameManager.selectionHandler.GetLastSelectedObject();

        if (selectedObject == null || !selectedObject.CompareTag("Playable"))
        {
            airtime.emptySlot(slot);
            cassetteSlotTexts[slot].text = "Slot " + (slot + 1);
            PlayCassetteSound();
            return;
        }

        if (selectedObject.CompareTag("Playable"))
        {
            playableObject = selectedObject.GetComponent<PlayableObject>();
            airtime.setSlot(playableObject.data, slot);
            cassetteSlotTexts[slot].text = playableObject.data.name + " U¿ycie: " + playableObject.data.GetTimesUsed();
            gameManager.selectionHandler.ResetLastSelectedObject();
            PlayCassetteSound();
        }
    }

    private void PlayCassetteSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(gameManager.putCasetteInSound, cassettePlayer.transform.position);
    }

    public void ToggleVisibility()
    {
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
        for(int i =0;i<cassetteSlotTexts.Length;i++)
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
