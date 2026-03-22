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

    [SerializeField] private float zOffsetMin = 0.5f;
    [SerializeField] private float zOffsetMax = 0.5f;

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
        Vector3 currentScreenPoint = new Vector3(mousePos.x, mousePos.y, screenPoint.z);
        Vector3 currentPosition = mainCamera.ScreenToWorldPoint(currentScreenPoint) + offset;

        //Zamienia globaln� pozycje gdzie ma by� suwak na pozycje lokaln� wzgl�dem rodzica suwaka (konsoli)
        Vector3 currentLocalPosition = transform.parent.InverseTransformPoint(currentPosition);

        float minZ = startingLocalPos.z - zOffsetMin;
        float maxZ = startingLocalPos.z + zOffsetMax;


        //POPRAWA  �EBY SLIDER DZIA�A� W DOWOLNYM MIEJSCY I W NIECO MNIEJSZYM ZAKRESIE MO�E
        float clampedZ = Mathf.Clamp(currentLocalPosition.z, minZ, maxZ);

        transform.localPosition = new Vector3(startingLocalPos.x, startingLocalPos.y, clampedZ);

        currentValue = Mathf.InverseLerp(maxZ, minZ, clampedZ);

    }

}
