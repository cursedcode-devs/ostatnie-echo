using UnityEngine;
using UnityEngine.InputSystem;

public class TimeLapseController : MonoBehaviour
{
    [Header("Ustawienia Time Lapse")]
    [Tooltip("Klawisz do włączania/wyłączania efektu Time Lapse")]
    public Key toggleTimeLapseKey = Key.Space;
    
    [Tooltip("Co ile sekund ma następować losowa zmiana?")]
    public float changeInterval = 0.1f;
    
    [Tooltip("Prawdopodobieństwo zmiany konkretnego elementu w każdym cyklu (0.0 - 1.0)")]
    public float chanceToChange = 0.5f;

    private bool isTimeLapseActive = false;
    private float timer = 0f;

    private ObjectToggler[] doorTogglers;
    private MultiObjectCycler[] cassetteCyclers;
    private StatsRandomizer statsRandomizer;

    void Start()
    {
        // find scene dependencies
        doorTogglers = FindObjectsOfType<ObjectToggler>();
        cassetteCyclers = FindObjectsOfType<MultiObjectCycler>();
        statsRandomizer = FindObjectOfType<StatsRandomizer>();
    }

    void Update()
    {
        bool pressed = false;

        if (Keyboard.current != null && Keyboard.current[toggleTimeLapseKey].wasPressedThisFrame)
        {
            pressed = true;
        }

        try
        {
            // legacy input fallback
            if (Input.GetKeyDown(KeyCode.Space)) pressed = true;
        }
        catch { }

        if (pressed)
        {
            isTimeLapseActive = !isTimeLapseActive;
            Debug.Log($"[TimeLapse] Status: {isTimeLapseActive}");
        }

        if (isTimeLapseActive)
        {
            timer += Time.deltaTime;
            if (timer >= changeInterval)
            {
                timer = 0f;
                DoRandomLapseStep();
            }
        }
    }

    private void DoRandomLapseStep()
    {
        // randomize doors
        if (doorTogglers != null)
        {
            foreach (var t in doorTogglers)
            {
                if (Random.value < chanceToChange) t.SetState(Random.value > 0.5f);
            }
        }

        // randomize cassettes
        if (cassetteCyclers != null)
        {
            foreach (var c in cassetteCyclers)
            {
                if (Random.value < chanceToChange) c.SetState(Random.Range(0, 3));
            }
        }

        // randomize displays
        if (statsRandomizer != null)
        {
            if (Random.value < chanceToChange) statsRandomizer.RandomizeStats();
        }
    }
}
