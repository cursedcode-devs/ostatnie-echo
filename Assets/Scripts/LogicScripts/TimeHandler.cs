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

        if (Day > 7)
        {
            FinishGame();
        }
    }
    public void FinishGame()
    {
    #if UNITY_STANDALONE
        Application.Quit();
    #endif
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;    
    #endif
    }
}