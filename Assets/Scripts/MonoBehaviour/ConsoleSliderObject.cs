using UnityEngine;
using UnityEngine.InputSystem;

public class ConsoleSliderObject : MonoBehaviour
{
    [Header("Dane z ScriptableObject")]
    public ConsoleSlider data;
    public Camera mainCamera;

    private Vector3 screenPoint;
    private Vector3 offset;

    void Start()
    {
        if(data != null)
        {
            gameObject.tag = "ConsoleSlider";
            Debug.Log("To jest suwak w konsoli: " + data.name);
        }

        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void OnMouseClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        screenPoint = mainCamera.WorldToScreenPoint(gameObject.transform.position);

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, screenPoint.z));
        offset = gameObject.transform.position - mouseWorldPos;

        Debug.Log("OnMouseDownSlider");
    }

    public void OnMousePressed()
    {
        Debug.Log("OnMouseDragSlider");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 currentScreenPoint = new Vector3(mousePos.x,mousePos.y, screenPoint.z);
        Vector3 currentPosition = mainCamera.ScreenToWorldPoint(currentScreenPoint) + offset;
        transform.position = new Vector3(transform.position.x, transform.position.y, currentPosition.z);
    }
   
}
