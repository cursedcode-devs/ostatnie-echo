using UnityEngine;
using    System;

[System.Serializable]
public class TimeHandler
{
    [SerializeField] private int Hour;
    [SerializeField] private int Day;

    public event Action OnDayStarted;

    public TimeHandler(int startHour, int startDay)
    {
        Hour = startHour;
        Day = startDay;
    }

    public void NextHour()
    {
        Hour++;
        if (Hour > 20)
        {
            StartDay();
        }
    }
    public void StartDay()
    {
        Day++;
        Hour = 14;

        OnDayStarted?.Invoke();
    }
    public bool FinishGame()
    {
        if (Day > 7)
        {
            return true;
        }

        return false;
    }
}