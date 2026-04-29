using UnityEngine;
using System;

[System.Serializable]
public class TimeHandler
{
    [SerializeField] private int Hour;
    [SerializeField] private int Day;

    public int CurrentDay => Day;
    public int CurrentHour => Hour;

    public event Action OnDayStarted;
    public event Action OnGameFinished;

    [SerializeField] private int lastDay = 2;

    public TimeHandler(int startHour, int startDay)
    {
        Hour = startHour;
        Day  = startDay;
    }

    public void NextHour()
    {
        Hour++;
        MiniGameSystem.Instance.LaunchRandom();
        if (Hour > 17)
            StartDay();
    }

    public void StartDay()
    {
        Day++;
        Hour = 14;

        OnDayStarted?.Invoke();

        if (Day > lastDay)
            OnGameFinished?.Invoke();
    }
}
