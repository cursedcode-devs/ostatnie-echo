using UnityEngine;
using System;

[System.Serializable]
public class TimeHandler
{
    [SerializeField] private int Hour;
    [SerializeField] private int Day;
    public Airtime airtime;
    public CassetteSlotHandler[] slots;

    public int CurrentDay => Day;
    public int CurrentHour => Hour;

    public event Action OnDayStarted;
    public event Action OnGameFinished;
    private int lastDay;
    public TimeHandler(int startHour, int startDay, Airtime airtime, CassetteSlotHandler[] slots, int dayNr)
    {
        Hour = startHour;
        Day = startDay;
        this.airtime = airtime;
        this.slots = slots;
        lastDay = dayNr;
    }

    public void NextHour()
    {
        Hour++;

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ResetBack2Back();

        if (Hour > 17)
            StartDay();
        else
            MiniGameSystem.Instance.LaunchRandom();
    }

    public void StartDay()
    {
        airtime.emptyAllSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].PutCassetteInShelf();
        }
        Day++;
        Hour = 14;
        OnDayStarted?.Invoke();

        if (Day > lastDay)
            OnGameFinished?.Invoke();

    }
    public int getDay()
    {
        return Day;
    }
}
