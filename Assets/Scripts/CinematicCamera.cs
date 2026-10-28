using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2.5f;
    public float smoothing = 5f;

    [Header("Speed Control")]
    public float baseScrollStep = 0.001f;
    public float altScrollStep = 1f;
    public float keySpeedIncrement = 15f;
    public float minimumMoveSpeed = 0f;
    public float maxMoveSpeed = 500f;

    [Header("Look Settings")]
    public float lookSensitivity = 0.2f;
    public float maxPitch = 90f;
    public float minPitch = -90f;

    private float pitch = 0f;
    private float yaw = 0f;

    private Vector3 targetPosition;
    private bool isControllingCamera = true;
    private float savedSpeed = -1f;

    void Start()
    {
        targetPosition = transform.position;
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;

        SetCameraControl(true);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (kb == null || mouse == null) return;

        // toggle cursor lock
        if (mouse.rightButton.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
        {
            SetCameraControl(!isControllingCamera);
        }

        if (!isControllingCamera) return;

        float currentLookSensitivity = lookSensitivity;
        if (mouse.leftButton.isPressed)
        {
            currentLookSensitivity *= 5f; // faster rotation on left click
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();
        yaw += mouseDelta.x * currentLookSensitivity;
        pitch -= mouseDelta.y * currentLookSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (kb.gKey.wasPressedThisFrame)
        {
            // save current speed
            savedSpeed = moveSpeed;
        }
        
        if (kb.fKey.wasPressedThisFrame)
        {
            // restore saved speed
            if (savedSpeed >= 0f)
            {
                moveSpeed = savedSpeed;
            }
        }

        // adjust speed with scroll or keys
        float scrollY = mouse.scroll.ReadValue().y;
        
        if (Mathf.Abs(scrollY) > 0.01f)
        {
            float normalizedScroll = Mathf.Sign(scrollY);
            float step = kb.altKey.isPressed ? altScrollStep : baseScrollStep;
            moveSpeed += normalizedScroll * step;
            
            // round to 3 decimals to reach zero cleanly
            moveSpeed = Mathf.Round(moveSpeed * 1000f) / 1000f;
        }

        float speedModifier = kb.altKey.isPressed ? 1f : 0.1f;
        if (kb.equalsKey.isPressed || kb.numpadPlusKey.isPressed)
        {
            moveSpeed += keySpeedIncrement * Time.deltaTime * speedModifier;
        }
        else if (kb.minusKey.isPressed || kb.numpadMinusKey.isPressed)
        {
            moveSpeed -= keySpeedIncrement * Time.deltaTime * speedModifier;
        }
        
        moveSpeed = Mathf.Clamp(moveSpeed, minimumMoveSpeed, maxMoveSpeed);

        float speed = moveSpeed;
        if (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed)
        {
            speed *= sprintMultiplier;
        }

        float horizontal = 0f;
        float vertical = 0f;
        
        if (kb.wKey.isPressed) vertical += 1f;
        if (kb.sKey.isPressed) vertical -= 1f;
        if (kb.dKey.isPressed) horizontal += 1f;
        if (kb.aKey.isPressed) horizontal -= 1f;

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        
        // vertical movement
        float verticalLevitation = 0f;
        if (kb.eKey.isPressed) verticalLevitation += 1f;
        if (kb.qKey.isPressed) verticalLevitation -= 1f;

        Vector3 localMove = (transform.right * inputDir.x) + (transform.forward * inputDir.z) + (Vector3.up * verticalLevitation);
        
        targetPosition += localMove * speed * Time.deltaTime;

        // smooth position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
    }

    private void SetCameraControl(bool controlActive)
    {
        isControllingCamera = controlActive;
        if (isControllingCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
