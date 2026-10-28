using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectToggler : MonoBehaviour
{
    [Header("Ustawienia Wejścia")]
    public Key toggleKey = Key.None;

    [Header("Obiekty do przełączania")]
    [Tooltip("Obiekt widoczny na początku (włączony domyślnie)")]
    public GameObject objectA;

    [Tooltip("Obiekt ukryty na początku (wyłączony domyślnie)")]
    public GameObject objectB;

    private bool isStateB = false;

    void Start()
    {
        // set initial state
        if (objectA != null) objectA.SetActive(true);
        if (objectB != null) objectB.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (toggleKey != Key.None && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        isStateB = !isStateB;
        ApplyState();
    }

    public void SetState(bool stateB)
    {
        isStateB = stateB;
        ApplyState();
    }

    private void ApplyState()
    {
        if (objectA != null) objectA.SetActive(!isStateB);
        if (objectB != null) objectB.SetActive(isStateB);
    }
}
