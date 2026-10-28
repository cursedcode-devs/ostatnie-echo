using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TrailerMultiCassette : MonoBehaviour
{
    [System.Serializable]
    public class CassetteData
    {
        [Tooltip("Obiekt kasety do poruszania")]
        public Transform cassetteTransform;
        
        [Tooltip("Klawisz wyzwalający wysunięcie (np. Digit2, Digit3)")]
        public Key triggerKey;
        
        [Tooltip("Pozycja Y (lokalna), gdy kaseta jest wciśnięta/schowana")]
        public float insertedY = 0.00003f;
        
        [Tooltip("Pozycja Y (lokalna), gdy kaseta jest wysunięta")]
        public float ejectedY = -0.001685f;
        
        [HideInInspector] public bool isEjected;
        [HideInInspector] public Coroutine animationRoutine;
    }

    [Header("Ustawienia Animacji")]
    [Tooltip("Czas trwania płynnego wysuwania/wsuwania w sekundach")]
    public float animationTime = 1.0f;

    [Header("Lista Kaset")]
    public CassetteData[] cassettes = new CassetteData[]
    {
        new CassetteData { triggerKey = Key.Digit2 },
        new CassetteData { triggerKey = Key.Digit3 },
        new CassetteData { triggerKey = Key.Digit4 },
        new CassetteData { triggerKey = Key.Digit5 }
    };

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        foreach (var cassette in cassettes)
        {
            if (cassette.cassetteTransform == null) continue;

            // check assigned trigger key
            if (kb[cassette.triggerKey].wasPressedThisFrame)
            {
                cassette.isEjected = !cassette.isEjected;
                
                // stop active animation if triggered again
                if (cassette.animationRoutine != null)
                {
                    StopCoroutine(cassette.animationRoutine);
                }
                
                float targetY = cassette.isEjected ? cassette.ejectedY : cassette.insertedY;
                cassette.animationRoutine = StartCoroutine(AnimateY(cassette, targetY, animationTime));
            }
        }
    }

    private IEnumerator AnimateY(CassetteData cassette, float endY, float duration)
    {
        Vector3 startPos = cassette.cassetteTransform.localPosition;
        float startY = startPos.y;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // smoothstep easing
            t = t * t * (3f - 2f * t); 

            float currentY = Mathf.Lerp(startY, endY, t);
            cassette.cassetteTransform.localPosition = new Vector3(startPos.x, currentY, startPos.z);
            yield return null;
        }

        // snap to target position
        cassette.cassetteTransform.localPosition = new Vector3(startPos.x, endY, startPos.z);
        cassette.animationRoutine = null;
    }
}
