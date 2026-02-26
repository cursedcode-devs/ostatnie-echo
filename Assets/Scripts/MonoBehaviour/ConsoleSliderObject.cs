using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class ConsoleSliderObject : MonoBehaviour
{
    [Header("Dane z ScriptableObject")]
    public ConsoleSlider data;
    public Camera mainCamera;

    private Vector3 screenPoint;
    private Vector3 offset;
    public UnityEvent<float> onValueChanged = new UnityEvent<float>();
    [SerializeField] private float currentValue=0f;

    
    void Start()
    {
        if(data != null)
        {
            gameObject.tag = "ConsoleSlider";
            Debug.Log("To jest suwak w konsoli: " + data.name);
        }

        if (mainCamera == null) mainCamera = Camera.main;
        Vector3 start = gameObject.transform.position;

    }


    public float GetCurrentValue()
    {
        return currentValue;
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
        Debug.Log(currentValue);
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 currentScreenPoint = new Vector3(mousePos.x, mousePos.y, screenPoint.z);
        Vector3 currentPosition = mainCamera.ScreenToWorldPoint(currentScreenPoint) + offset;

        float minZ = -7.5f;
        float maxZ = -6.5f;

        float clampedZ = Mathf.Clamp(currentPosition.z, minZ, maxZ);
        transform.position = new Vector3(transform.position.x, transform.position.y, clampedZ);

        float newValue = Mathf.InverseLerp(minZ, maxZ, clampedZ);

        if (Mathf.Abs(newValue - currentValue) > 0.0001f)
        {
            currentValue = newValue;
            Debug.Log("Invoke event: " + currentValue);
            onValueChanged.Invoke(currentValue);
        }
    }

}
