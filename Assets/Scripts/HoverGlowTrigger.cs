using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(SimpleGlowOutline))]
public class HoverGlowTrigger : MonoBehaviour
{
    private Collider col;
    private SimpleGlowOutline outline;
    private Camera cam;

    void Start()
    {
        col = GetComponent<Collider>();
        outline = GetComponent<SimpleGlowOutline>();
        cam = Camera.main;
        outline.enabled = false;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        
        var mouse = Mouse.current;
        if (mouse == null) return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        bool hovered = false;
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.collider == col)
            {
                hovered = true;
            }
        }

        if (outline.enabled != hovered)
        {
            outline.enabled = hovered;
        }
    }
}
