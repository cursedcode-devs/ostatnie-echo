using UnityEngine;

[System.Serializable]
public struct GenreValues
{
    public int hipHop;
    public int disco;
    public int rock;
    public int pop;
    public int totalListeners => hipHop + disco + rock + pop;


    public override string ToString(){
        return $"Hip Hop: {hipHop}\nDisco: {disco}\nRock: {rock}\nMetal: {pop}";
    }
}
