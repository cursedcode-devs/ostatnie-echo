using UnityEngine;
using UnityEngine.InputSystem;

public class MultiObjectCycler : MonoBehaviour
{
    [Header("Klawisze")]
    public Key toggleKey = Key.None;
    public Key toggleKeyNumpad = Key.None;

    [Header("Obiekty do sekwencji")]
    public GameObject object1;
    public GameObject object2;

    // 0 off 1 object1 2 object2
    private int state = 0; 

    void Start()
    {
        // set initial state
        UpdateState();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        bool pressed = false;
        if (toggleKey != Key.None && Keyboard.current[toggleKey].wasPressedThisFrame) pressed = true;
        if (toggleKeyNumpad != Key.None && Keyboard.current[toggleKeyNumpad].wasPressedThisFrame) pressed = true;

        if (pressed)
        {
            state = (state + 1) % 3;
            UpdateState();
        }
    }

    private void UpdateState()
    {
        if (object1 != null) object1.SetActive(state == 1);
        if (object2 != null) object2.SetActive(state == 2);
    }

    public void SetState(int newState)
    {
        state = newState % 3;
        UpdateState();
    }
}
