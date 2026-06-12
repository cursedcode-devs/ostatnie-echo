using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class ZoomTarget
{
    [Tooltip("Obiekt, na który gracz musi kliknąć (musi posiadać Collider).")]
    public Collider interactableCollider;

    [Tooltip("Pusty obiekt określający gdzie ma znaleźć się kamera po przybliżeniu. Jeśli puste, kamera po prostu ustawi się przed obiektem.")]
    public Transform targetCameraPosition;

    [Tooltip("Komponent odpowiedzialny za obrys/podświetlenie (np. skrypt Outline, Halo lub inny efekt). Zostanie włączony przy najechaniu.")]
    public Behaviour outlineComponent;

    [Header("Opcjonalne zdarzenia (np. do zmiany koloru, włączenia UI)")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    
}

public class ZoomHandler : MonoBehaviour
{
    [Header("Ustawienia Kamery")]
    [Tooltip("Jeśli puste, użyje Camera.main")]
    public Camera mainCamera;
    public FMODUnity.EventReference zoomSound;
    public FMODUnity.EventReference zoomOutSound;
    [Tooltip("Czas trwania animacji przybliżania i oddalania (w sekundach).")]
    public float zoomDuration = 1.0f;
    
    [Tooltip("Krzywa animacji zapewniająca płynny start i koniec (ease-in, ease-out).")]
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("Odległość, na jakiej zatrzyma się kamera, jeśli nie podano 'targetCameraPosition'.")]
    public float autoZoomDistance = 1.5f;

    [Header("Lista Obiektów Interaktywnych")]
    public List<ZoomTarget> interactableObjects = new List<ZoomTarget>();

    // Stan kamery i systemu
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isZoomedIn = false;
    private bool isAnimating = false;
    
    private ZoomTarget currentHoveredTarget = null;
    private ZoomTarget currentZoomedTarget = null;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        // Upewniamy się, że wszystkie podświetlenia są na start wyłączone
        foreach (var target in interactableObjects)
        {
            if (target.outlineComponent != null)
                target.outlineComponent.enabled = false;
        }
    }

    void Update()
    {
        // Blokujemy interakcje podczas animacji kamery
        if (isAnimating) return;

        if (!isZoomedIn)
        {
            HandleHover();

            // Lewy przycisk myszy - zoom in
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && currentHoveredTarget != null)
            {
                ZoomIn(currentHoveredTarget);
            }
        }
        else
        {
            // Prawy przycisk myszy - zoom out (powrót)
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                ZoomOut();
            }
        }
    }

    private void HandleHover()
    {
        if (mainCamera == null) return;

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        ZoomTarget hitTarget = null;

        // Strzelamy promieniem z myszki by sprawdzić czy celujemy w Collider z naszej listy
        if (Physics.Raycast(ray, out hit))
        {
            hitTarget = interactableObjects.Find(x => x.interactableCollider == hit.collider);
        }

        // Jeśli zmieniliśmy obiekt, na który najeżdżamy myszką
        if (hitTarget != currentHoveredTarget)
        {
            // 1. Wygaszamy stary obiekt
            if (currentHoveredTarget != null)
            {
                if (currentHoveredTarget.outlineComponent != null)
                    currentHoveredTarget.outlineComponent.enabled = false;
                    
                currentHoveredTarget.onHoverExit?.Invoke();
            }

            currentHoveredTarget = hitTarget;

            // 2. Zapalamy nowy obiekt
            if (currentHoveredTarget != null)
            {
                if (currentHoveredTarget.outlineComponent != null)
                    currentHoveredTarget.outlineComponent.enabled = true;
                    
                currentHoveredTarget.onHoverEnter?.Invoke();
            }
        }
    }

    private void ZoomIn(ZoomTarget target)
    {
        if (isAnimating || isZoomedIn) return;
        
        // Zapisujemy pozycję i rotację startową kamery, by mieć do czego wracać
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        currentZoomedTarget = target;
        isZoomedIn = true;

        Vector3 targetPos;
        Quaternion targetRot;

        // Obliczamy pozycję docelową
        if (target.targetCameraPosition != null)
        {
            targetPos = target.targetCameraPosition.position;
            targetRot = target.targetCameraPosition.rotation;
        }
        else
        {
            // Automatyczne przybliżenie, jeśli nie podano ręcznie ustawionego punktu
            Vector3 objectCenter = target.interactableCollider.bounds.center;
            Vector3 directionToCamera = (originalCameraPosition - objectCenter).normalized;
            targetPos = objectCenter + directionToCamera * autoZoomDistance;
            targetRot = Quaternion.LookRotation(objectCenter - targetPos);
        }
        FMODUnity.RuntimeManager.PlayOneShot(zoomSound, this.transform.position);
        // Wyłączamy outline podczas "oglądania"
        if (target.outlineComponent != null)
            target.outlineComponent.enabled = false;
        target.onHoverExit?.Invoke();
        currentHoveredTarget = null; // reset hover

        StartCoroutine(AnimateCamera(targetPos, targetRot));
    }

    public void ZoomOut(bool instantZoom=false)
    {
        if (!isZoomedIn) return;
        
        if (instantZoom)
        {
            mainCamera.transform.position = originalCameraPosition;
            mainCamera.transform.rotation = originalCameraRotation;
        }
        else
        {
            if (isAnimating) return;

            StartCoroutine(AnimateCamera(originalCameraPosition, originalCameraRotation));
        }
        FMODUnity.RuntimeManager.PlayOneShot(zoomOutSound, this.transform.position);
        isZoomedIn = false;
        currentZoomedTarget = null;
    }

    /// <summary>
    /// Programowe przybliżenie kamery do zadanej pozycji/rotacji (np. na czas minigry).
    /// Powrót do pozycji wyjściowej: ZoomOut().
    /// </summary>
    public void ZoomToTransform(Transform camTarget)
    {
        if (camTarget == null) return;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Zapisz pozycję wyjściową TYLKO jeśli nie jesteśmy jeszcze przybliżeni —
        // żeby ZoomOut wrócił do prawdziwego punktu startowego, nawet gdy gracz był
        // już przybliżony gdzie indziej.
        if (!isZoomedIn)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }

        isZoomedIn = true;
        currentZoomedTarget = null;

        // Wyłącz ewentualny hover
        if (currentHoveredTarget != null)
        {
            if (currentHoveredTarget.outlineComponent != null)
                currentHoveredTarget.outlineComponent.enabled = false;
            currentHoveredTarget.onHoverExit?.Invoke();
            currentHoveredTarget = null;
        }

        // Przerwij ewentualną trwającą animację i animuj na nowo do konsolety.
        StopAllCoroutines();
        isAnimating = false;
        StartCoroutine(AnimateCamera(camTarget.position, camTarget.rotation));
    }

    /// <summary>True jeśli kamera jest aktualnie przybliżona.</summary>
    public bool IsZoomedIn => isZoomedIn;

    private IEnumerator AnimateCamera(Vector3 targetPosition, Quaternion targetRotation)
    {
        isAnimating = true;

        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Procent czasu z nałożeniem krzywej dla efektu ease-in ease-out
            float t = Mathf.Clamp01(elapsedTime / zoomDuration);
            float curveT = zoomCurve.Evaluate(t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveT);

            yield return null;
        }

        // Gwarancja idealnego ustawienia na koniec animacji
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isAnimating = false;
    }
}
