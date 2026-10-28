using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CassetteAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Obiekt do poruszania. Jeśli puste, użyje obiektu z tym skryptem.")]
    public Transform targetObject;
    
    [Tooltip("Docelowa pozycja Y (lokalna)")]
    public float targetY = 0.0097f;
    
    [Tooltip("Czas trwania animacji w sekundach")]
    public float animationTime = 1.5f;

    private bool isAnimating = false;

    void Start()
    {
        if (targetObject == null)
        {
            targetObject = transform;
        }
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        
        // trigger animation on key 1
        if (kb != null && kb.digit1Key.wasPressedThisFrame && !isAnimating)
        {
            StartCoroutine(AnimateY(targetY, animationTime));
        }
    }

    private IEnumerator AnimateY(float endY, float duration)
    {
        isAnimating = true;
        
        Vector3 startPos = targetObject.localPosition;
        Vector3 endPos = new Vector3(startPos.x, endY, startPos.z);
        
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            float t = elapsedTime / duration;
            // smoothstep easing
            t = t * t * (3f - 2f * t);

            targetObject.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // snap to target position
        targetObject.localPosition = endPos;
        isAnimating = false;
    }
}
