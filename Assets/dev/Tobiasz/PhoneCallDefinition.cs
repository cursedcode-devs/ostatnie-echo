using UnityEngine;
using System.Collections.Generic;

public enum PhoneCallType
{
    SongRequest,
    DialogOptions
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
