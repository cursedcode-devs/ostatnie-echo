using UnityEngine;

public class CassetteSlotHandler : MonoBehaviour
{
    public int slotID;
    public Airtime airtime;
    private bool isOpen = false;
    public Transform hinge;


    private Vector3 closedRotation = new Vector3(0f, 0f, 0f);
    private Vector3 openRotation = new Vector3(-125f, 0f, 0f);
    public void HandleSlot()
    {
        if (!isOpen)
        {
            hinge.localRotation = Quaternion.Euler(openRotation);
            isOpen = true;
        }
        else if (isOpen)
        {
            hinge.localRotation = Quaternion.Euler(closedRotation);
            isOpen = false;
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
