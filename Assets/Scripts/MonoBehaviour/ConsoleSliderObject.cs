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
    private Vector3 startingLocalPos;   //Lokalna pozycja wzgl�dem konsoli

    public float xOffsetMin = 0.014f;
    public float xOffsetMax = 0.014f;

    [SerializeField] private float currentValue = 0f;
    public FMODUnity.EventReference clickSound;

    void Start()
    {
        if (data != null)
        {
            tag = "ConsoleSlider";
            Debug.Log("To jest suwak w konsoli: " + data.name);
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        startingLocalPos = transform.localPosition;

        currentValue = 0.50f; 
    }

    public float GetCurrentValue()
    {
        return currentValue;
    }

    public void OnMouseClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        screenPoint = mainCamera.WorldToScreenPoint(transform.position);
        FMODUnity.RuntimeManager.PlayOneShot(clickSound, this.transform.position);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, screenPoint.z));

        offset = transform.position - mouseWorldPos;

        Debug.Log("OnMouseDownSlider");
    }

    public void OnMousePressed()
    {
        Debug.Log(currentValue);
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Głębia jest już naprawiona (screenPoint.z), więc nie będzie teleportów
        Vector3 currentScreenPoint = new Vector3(mousePos.x, mousePos.y, screenPoint.z);
        Vector3 currentPosition = mainCamera.ScreenToWorldPoint(currentScreenPoint) + offset;

        Vector3 currentLocalPosition = transform.parent.InverseTransformPoint(currentPosition);

        // Skoro Z to lewo-prawo, a Y to góra-dół, wracamy do lokalnego X (czyli Twój globalny X)
        float minX = startingLocalPos.x - xOffsetMin;
        float maxX = startingLocalPos.x + xOffsetMax;

        float clampedX = Mathf.Clamp(currentLocalPosition.x, minX, maxX);

        // Aplikujemy pozycję wyłącznie do parametru X
        transform.localPosition = new Vector3(clampedX, startingLocalPos.y, startingLocalPos.z);

        currentValue = Mathf.InverseLerp(maxX, minX, clampedX);
    }



}
