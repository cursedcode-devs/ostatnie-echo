using UnityEngine;

public class CassetteSlotHandler : MonoBehaviour
{
    public int slotID;
    public Airtime airtime;
    private bool isOpen = false;
    public Transform hinge;
    public GameObject SlotHitBox;
    public ObjectSelectionHandler selectionHandler;
    private GameObject cassette;
    private Vector3 startPos = Vector3.zero;
    private Quaternion startRot = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    private Vector3 closedRotation = new Vector3(0f, 0f, 0f);
    private Vector3 openRotation = new Vector3(-125f, 0f, 0f);
    public void HandleHinge()
    {
        if (!isOpen)
        {
            hinge.localRotation = Quaternion.Euler(openRotation);
            isOpen = true;
            SlotHitBox.SetActive(true);
        }
        else if (isOpen)
        {
            hinge.localRotation = Quaternion.Euler(closedRotation);
            isOpen = false;
            SlotHitBox.SetActive(false);
        }
    }

    public void PutCassetteOut()
    {
        if (cassette == null)
            return;

        selectionHandler.SelectObject(cassette);
        selectionHandler.SetDeselectPos(startPos);
        selectionHandler.SetDeselectRot(startRot);
        cassette = null;
        airtime.emptySlot(slotID);
    }

    public void PutCassetteInShelf()
    {
        cassette.transform.position = startPos;
        cassette.transform.rotation = startRot;
    }

    public bool PutCassetteIn(GameObject cassette)
    {
        if (airtime.GetCassettes()[slotID] == null)
        {
            this.cassette = cassette;
            startPos = selectionHandler.GetDesPos();
            startRot = selectionHandler.GetDesRot();
            cassette.transform.position = SlotHitBox.transform.position;
            cassette.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
            //cassette.transform.rotation = SlotHitBox.transform.rotation;
            airtime.setSlot(cassette.GetComponent<PlayableObject>().data, slotID);

            return true;
        }
        //jeœli slot jest zajêty to nie mo¿na w³o¿yæ kasety

        return false;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SlotHitBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
