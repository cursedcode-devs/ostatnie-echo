using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StatsRandomizer : MonoBehaviour
{
    [Header("Wyświetlacze (każdy z nich pokaże 4 linie)")]
    public TMP_Text stats1;
    public TMP_Text stats2;
    public TMP_Text stats3;
    public TMP_Text statsSum;

    void Start()
    {
        // randomize once on start
        RandomizeStats();
    }

    void Update()
    {
        bool pressed = false;

        // new input system
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            pressed = true;
        }

        // legacy input fallback
        try
        {
            if (Input.GetKeyDown(KeyCode.Tab)) pressed = true;
        }
        catch { }

        // tab key trigger
        if (pressed)
        {
            Debug.Log("[StatsRandomizer] Tab pressed, randomizing stats");
            RandomizeStats();
        }
    }

    public void RandomizeStats()
    {
        // generate random values for each connected display
        if (stats1 != null) { stats1.text = GenerateMultilineStats(); stats1.ForceMeshUpdate(); }
        if (stats2 != null) { stats2.text = GenerateMultilineStats(); stats2.ForceMeshUpdate(); }
        if (stats3 != null) { stats3.text = GenerateMultilineStats(); stats3.ForceMeshUpdate(); }
        if (statsSum != null) { statsSum.text = GenerateMultilineStats(); statsSum.ForceMeshUpdate(); }
    }

    private string GenerateMultilineStats()
    {
        int v1 = Random.Range(-100, 101);
        int v2 = Random.Range(-100, 101);
        int v3 = Random.Range(-100, 101);
        int v4 = Random.Range(-100, 101);

        return $"{FormatStat("Hip Hop: ", v1)}\n" +
               $"{FormatStat("Rock: ", v2)}\n" +
               $"{FormatStat("Pop: ", v3)}\n" +
               $"{FormatStat("Disco Polo: ", v4)}";
    }

    private string FormatStat(string prefix, int value)
    {
        string sign = value > 0 ? "+" : "";
        // format zero without sign
        if (value == 0) return $"{prefix}0%";
        
        return $"{prefix}{sign}{value}%";
    }
}
