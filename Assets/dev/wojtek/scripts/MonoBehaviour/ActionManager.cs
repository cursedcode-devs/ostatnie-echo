using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bierze input i zamienia go na rodzaj konkretnych akcji w grze
/// </summary>
public class ActionManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private Vector3 mousePosition;
    private GameObject clickedObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetClickedObject()
    {
        return clickedObject;
    }

    public ActionTypes actionType()
    {
        mousePosition = Mouse.current.position.ReadValue();
        bool leftMousePressed = Mouse.current.leftButton.wasPressedThisFrame;

        if (!leftMousePressed)
        {
            return ActionTypes.None;
        }
        clickedObject = ClickedObject(mousePosition);
        if (clickedObject != null)
        {
            return ActionTypes.LeftClickOnObject;
        }
        else
        {
            return ActionTypes.LeftClickOutsiedObject;
        }
    }

    public GameObject ClickedObject(Vector3 mousePos)
    {
        Ray clickingRay = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit raycastHit;
        bool objectHited = Physics.Raycast(clickingRay, out raycastHit);

        if (objectHited)
        {
            Debug.Log(raycastHit.transform.name);
        }

        if (!objectHited) //Nie klikniêto w obiekt z colliderem
        {
            return null;
        }
 
        if (objectHited)  //klikniêto w obiekt z colliderem
        {
            return raycastHit.transform.gameObject;   
        }
        return null;
    }
}
