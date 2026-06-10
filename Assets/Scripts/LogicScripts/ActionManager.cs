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

    public ActionTypes GetKeyboardActionType()
    {
        if (Keyboard.current.aKey.IsPressed())
            return ActionTypes.PressedA;
        if (Keyboard.current.dKey.IsPressed())
            return ActionTypes.PressedD;
        if (Keyboard.current.wKey.IsPressed())
            return ActionTypes.PressedW;
        if (Keyboard.current.sKey.IsPressed())
            return ActionTypes.PressedS;
        if (Keyboard.current.qKey.IsPressed())
            return ActionTypes.PressedQ;
        if (Keyboard.current.eKey.IsPressed())
            return ActionTypes.PressedE;
        if (Keyboard.current.enterKey.wasPressedThisFrame)
            return ActionTypes.PressedEnter;
        if (Keyboard.current.pKey.wasPressedThisFrame)
            return ActionTypes.PressedP;

        return ActionTypes.None;
    }

    public ActionTypes GetActionMouseType()
    {
        if (!Mouse.current.leftButton.isPressed)
        {
            return ActionTypes.None;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            return GetLeftClickAction();
        }

        if (pointedObject != null)
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

        if (pointedObject == null) 
            return ActionTypes.LeftClickOutsiedObject;

        if (pointedObject.CompareTag("Unselectable"))
            return ActionTypes.LeftClickOutsiedObject;

        if (pointedObject.CompareTag("Playable"))
            return ActionTypes.LeftClickOnPlayableObject;

        if (pointedObject.CompareTag("ConsoleSlider"))
            return ActionTypes.LeftClickOnSlider;

        if(pointedObject.CompareTag("Slot"))
            return ActionTypes.LeftClickOnSlotHinge;
        if(pointedObject.CompareTag("SlotHitBox"))
            return ActionTypes.LeftClickOnSlotHitBox;
        if (pointedObject.CompareTag("Shelf"))
            return ActionTypes.LeftClickOnShelf;
        if(pointedObject.CompareTag("PlayButton"))
            return ActionTypes.LeftClickOnPlayButton;
        if(pointedObject.CompareTag("SkipButton"))
            return ActionTypes.LeftClickOnSkipButton;


        if (pointedObject.CompareTag("CassettePlayer")) 
            return ActionTypes.LeftClickOnPlayingObject;

        return ActionTypes.LeftClickOnObject;
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
