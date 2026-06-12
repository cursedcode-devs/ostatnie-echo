using UnityEngine;
using System.Collections.Generic;

public enum PhoneCallType
{
    SongRequest,
    DialogOptions
}

/// <summary>
/// Trzy osie zakończenia gry. Każda to prosty licznik (int) w GameManager,
/// inkrementowany/dekrementowany przez wybory w dialogach telefonów.
///   Host       — Prowadzący: czy widownia pomoże mu uciec do bunkra.
///   Listener   — Słuchacz: czy subkultury się jednoczą, czy walczą.
///   Government — Rząd: czy uda się nadać sygnał ewakuacyjny.
/// None = wybór nie wpływa na żaden licznik.
/// </summary>
public enum EndingMeter
{
    None,
    Host,
    Listener,
    Government
}

[System.Serializable]
public class PhoneCallDialogOption
{
    public string optionText;
    [TextArea(2, 5)]
    public string resultingText;
    
    [Header("Rewards / Penalties")]
    public float moneyChange;
    public float listenersPrecentageChange; // e.g. -0.02 for -2%
        public string requestedGenre;
public GenreValues flatListenersChange;

    [Header("Wpływ na zakończenie (osie a/b)")]
    [Tooltip("Prowadzący. Dodatnie -> wariant a) (przeżywa), ujemne -> b) (ginie).")]
    public int hostDelta = 0;
    [Tooltip("Słuchacz. Dodatnie -> a) (subkultury jednoczą się), ujemne -> b) (walki).")]
    public int listenerDelta = 0;
    [Tooltip("Rząd. Dodatnie -> a) (sygnał ewakuacyjny), ujemne -> b) (cisza).")]
    public int governmentDelta = 0;
}

[CreateAssetMenu(fileName = "New Phone Call", menuName = "Radio/Phone Call")]
public class PhoneCallDefinition : ScriptableObject
{
    public PhoneCallType callType;
    public string callerName;
    
    [TextArea(3, 10)]
    public string initialDialog;

    [Header("For Dialog Options")]
    public List<PhoneCallDialogOption> dialogOptions;
}
