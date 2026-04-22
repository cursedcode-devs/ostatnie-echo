using UnityEngine;

[System.Serializable]
public struct GenreValues
{
    public int hipHop;
    public int disco;
    public int rock;
    public int metal;
    public string type;
    public int totalListeners => hipHop + disco + rock + metal;
}
