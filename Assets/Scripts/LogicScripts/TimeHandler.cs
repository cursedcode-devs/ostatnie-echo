using UnityEngine;

[System.Serializable]
public class TimeHandler
{
    [SerializeField] private int Hour;
    [SerializeField] private int Day;

    public TimeHandler(int startHour, int startDay)
    {
        Hour = startHour;
        Day = startDay;
    }

    public void NextHour(RadioStation radioStation, float startingModifier)
    {
        radioStation.SetHourlyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetHourlyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        Hour++;
        if (Hour > 20)
        {
            radioStation.SetDailyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
            radioStation.SetDailyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);
            NextDay();
        }
    }
    public void NextDay()
    {
        Day++;
        Hour = 14;
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