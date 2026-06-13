using System;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TimeHandler
{
    [SerializeField] private int Hour;
    [SerializeField] private int Day;
    public ZoomHandler zoomHandler;
    public Airtime airtime;
    public CassetteSlotHandler[] slots;

    public int CurrentDay => Day;
    public int CurrentHour => Hour;

    public event Action OnDayStarted;
    public event Action OnGameFinished;
    private int lastDay;
    private int lastHour;
    public TimeHandler(int startHour, int startDay, Airtime airtime, CassetteSlotHandler[] slots, ZoomHandler zoomHandler, int dayNr, int lastHour=16)
    {
        Hour = startHour;
        Day = startDay;
        this.airtime = airtime;
        this.slots = slots;
        lastDay = dayNr;
        this.zoomHandler = zoomHandler;
        this.lastHour = lastHour;
    }

    public void NextHour()
    {
        Hour++;

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ResetBack2Back();

        if (Hour > lastHour)
            StartDay();
        else
            MiniGameSystem.Instance.LaunchRandom();
    }

    public void StartDay()
    {
        lastHour++;
        zoomHandler.ZoomOut(true);
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
