using UnityEngine;

[System.Serializable]
public struct GenreListeners
{
    public int hipHop;
    public int disco;
    public int rock;
    public int metal;

    public int totalListeners => hipHop + disco + rock + metal;
}
