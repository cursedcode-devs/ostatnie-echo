using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UIElements.UxmlAttributeDescription;

/// <summary>
/// Bierze input i zamienia go na rodzaj konkretnych akcji w grze
/// </summary>
public class ActionManager
{
    private Camera mainCamera;
    private GameObject pointedObject;

    public ActionManager(Camera mainCamera)
    {
        this.mainCamera = mainCamera;
    }

    public GameObject GetPointedObject()
    {
        return pointedObject;
    }

    public ActionTypes GetActionType()
    {
        if (!Mouse.current.leftButton.isPressed)
        {
            return ActionTypes.None;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            return GetLeftClickAction();
        }

        if(pointedObject != null)
        {
            if (pointedObject.CompareTag("ConsoleSlider"))
            {
                return ActionTypes.LeftPressedOnSlider;
            }
        }

        return ActionTypes.None;

    }

    private ActionTypes GetLeftClickAction()
    {
        Debug.Log("GetActionType - LeftCLick");
        pointedObject = PointedObject(Mouse.current.position.ReadValue());

        if (pointedObject == null) return ActionTypes.LeftClickOutsiedObject;

        if (pointedObject.CompareTag("Playable")) return ActionTypes.LeftClickOnPlayableObject;

        if (pointedObject.CompareTag("ConsoleSlider"))
        {
            return ActionTypes.LeftClickOnSlider;
        }

        if (pointedObject.CompareTag("CassettePlayer")) return ActionTypes.LeftClickOnPlayingObject;

        return ActionTypes.None;
    }
        
    public GameObject PointedObject(Vector3 mousePos)
    {
        Ray clickingRay = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(clickingRay, out RaycastHit raycastHit))
        {
            Debug.Log(raycastHit.transform.name);
            return raycastHit.transform.gameObject;
        }
        return null;
    }
}
