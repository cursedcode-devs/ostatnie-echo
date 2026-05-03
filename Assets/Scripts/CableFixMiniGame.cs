using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// CableFixMiniGame — core logic, plain MonoBehaviour like LightsOutMiniGame.
/// CableFixMiniGameAdapter wraps it into the BaseMiniGame system.
///
/// 3 rounds. Each round:
///   - A bar bounces up and down inside a vertical play area
///   - The zone (marked by two cable dots on left/right walls) is centred at a random height
///   - Zone is always larger than the bar — fair but shrinking each round
///   - Speed increases each round
/// Player presses Space to freeze the bar. Win if bar fits inside the zone.
/// </summary>
public class CableFixMiniGame : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Round definitions — matches the prototype
    // Round 0: slow,  big bar,    generous zone
    // Round 1: mid,   mid bar,    mid zone
    // Round 2: fast,  small bar,  tight-but-fair zone
    private struct RoundDef
    {
        public float speed;
        public float barHeight;   // fraction of play area height
        public float zoneHeight;  // fraction of play area height — always > barHeight
    }

    private static readonly RoundDef[] Rounds = new RoundDef[]
    {
        new RoundDef { speed = 220f, barHeight = 0.18f, zoneHeight = 0.33f },
        new RoundDef { speed = 360f, barHeight = 0.14f, zoneHeight = 0.26f },
        new RoundDef { speed = 520f, barHeight = 0.11f, zoneHeight = 0.20f },
    };

    // Possible zone centre positions as fractions of play area height
    private static readonly float[] ZoneCentres = { 0.28f, 0.40f, 0.55f, 0.65f, 0.74f };

    // ------------------------------------------------------------------
    // Fired when all rounds finish — CableFixMiniGameAdapter listens to this
    public event System.Action OnWin;

    // Wired by CableFixSceneBuilder after building UI
    public CableFixUI uiScript;

    // ------------------------------------------------------------------
    private int round = 0;
    private int successCount = 0;
    private bool active = false;
    private bool stopped = false;

    // Bar state (normalised 0-1 within play area)
    private float barPos = 0.5f;   // centre of bar, fraction of play area
    private float barDir = 1f;
    private float curSpeed = 220f;
    private float curBarH = 0.18f;
    private float zoneTop = 0f;
    private float zoneBot = 1f;

    // ------------------------------------------------------------------
    public void StartGame()
    {
        round = 0;
        successCount = 0;
        active = true;
        stopped = false;
        StartRound();
    }

    void StartRound()
    {
        var def = Rounds[round];
        curSpeed = def.speed;
        curBarH  = def.barHeight;

        float centreF = ZoneCentres[(round * 2 + 1) % ZoneCentres.Length];
        float halfZone = def.zoneHeight / 2f;
        zoneTop = Mathf.Clamp01(centreF - halfZone);
        zoneBot = Mathf.Clamp01(centreF + halfZone);

        barPos = 0.5f;
        barDir = 1f;
        stopped = false;

        uiScript?.SetRound(round, zoneTop, zoneBot, curBarH, curSpeed);
        uiScript?.SetMessage($"runda {round + 1} / {Rounds.Length}");
        Debug.Log($"[CableFixMiniGame] Round {round + 1} started. Speed:{curSpeed} BarH:{curBarH:F2} Zone:{zoneTop:F2}-{zoneBot:F2}");
    }

    // ------------------------------------------------------------------
    void Update()
    {
        if (!active || stopped) return;

        // Move bar
        barPos += curSpeed * barDir * Time.deltaTime / GetPlayAreaHeight();

        float halfBar = curBarH / 2f;
        if (barPos - halfBar < 0f) { barPos = halfBar; barDir = 1f; }
        if (barPos + halfBar > 1f) { barPos = 1f - halfBar; barDir = -1f; }

        uiScript?.SetBarPosition(barPos);

        // Input
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            TryStop();
    }

    void TryStop()
    {
        stopped = true;
        float halfBar = curBarH / 2f;
        bool hit = (barPos - halfBar) >= zoneTop && (barPos + halfBar) <= zoneBot;

        if (hit) successCount++;
        uiScript?.ShowResult(hit);
        uiScript?.SetPip(round, hit);

        Debug.Log($"[CableFixMiniGame] Round {round + 1} — {(hit ? "HIT" : "MISS")} barPos:{barPos:F2} zone:{zoneTop:F2}-{zoneBot:F2}");
        StartCoroutine(NextRoundDelay(hit));
    }

    IEnumerator NextRoundDelay(bool hit)
    {
        yield return new WaitForSeconds(0.9f);
        round++;
        if (round >= Rounds.Length)
        {
            active = false;
            uiScript?.SetMessage(successCount == Rounds.Length ? "sygnał przywrócony!" :
                                 successCount > 0              ? $"{successCount}/{Rounds.Length} — kabel niestabilny" :
                                                                 "naprawa nieudana");
            yield return new WaitForSeconds(0.6f);
            OnWin?.Invoke();
        }
        else
        {
            StartRound();
        }
    }

    // Returns play area height in world/screen units — UI script provides this
    float GetPlayAreaHeight() => uiScript != null ? uiScript.PlayAreaHeight : 600f;
}
