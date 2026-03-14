using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; 

public class ActionManager
{
    private Camera mainCamera;
    private GameObject clickedObject;

    public ActionManager(Camera mainCamera)
    {
        this.mainCamera = mainCamera;
    }

    public GameObject GetClickedObject()
    {
        return clickedObject;
    }

    public ActionTypes GetActionType()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return ActionTypes.None;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return ActionTypes.None;

        Debug.Log("GetActionType - LeftCLick");
        clickedObject = ClickedObject(Mouse.current.position.ReadValue());

        return clickedObject != null
            ? ActionTypes.LeftClickOnObject
            : ActionTypes.LeftClickOutsiedObject;
    }

    public GameObject ClickedObject(Vector3 mousePos)
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