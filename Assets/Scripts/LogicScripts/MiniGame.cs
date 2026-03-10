using UnityEngine;

[System.Serializable]
public abstract class MiniGame
{
    public abstract void Play();
    public abstract void Stop();
    public abstract void AddModifier(RadioStation radioStation);
    public abstract bool CheckWinCondition();
}