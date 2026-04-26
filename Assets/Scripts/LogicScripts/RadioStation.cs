using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa zajmuj�ca si� prezchowywanie stanu radio i zmienianiem go
/// </summary>
[System.Serializable]
public class RadioStation
{
    public GenreValues currentListeners;

    [SerializeField] private int startListeners = 5;
    [SerializeField] private float currentMoney = 0.00f;
    [SerializeField] private GenreValuesModifier hourlyListenersModifier;
    [SerializeField] private GenreValuesModifier hourlyRevenueModifier;
    [SerializeField] private GenreValuesModifier dailyListenersModifier;
    [SerializeField] private GenreValuesModifier dailyRevenueModifier;
    private float defaultModifier = 1f;
    private float defaultHourlyAndDailyModifier = 0f;
    [SerializeField] private GenreValuesModifier totalListenerModifer;
    [SerializeField] private GenreValuesModifier totalRevenueModifier;

    public void ApplySegment(GenreValues[] cassettes)
    {
        int totalHipHopListeners = 0;
        int totalDiscoListeners = 0;
        int totalRockListeners = 0;
        int totalMetalListeners = 0;

        int totalHipHopRevenue = 0;
        int totalDiscoRevenue = 0;
        int totalRockRevenue = 0;
        int totalMetalRevenue = 0;

        foreach (var cassette in cassettes)
        {
            switch (cassette.type)
            {
                case CassetteTypes.Music:
                    totalHipHopListeners += cassette.hipHop;
                    totalDiscoListeners += cassette.disco;
                    totalRockListeners += cassette.rock;
                    totalMetalListeners += cassette.metal;
                    break;
                case CassetteTypes.Ad:
                    totalHipHopRevenue += cassette.hipHop;
                    totalDiscoRevenue += cassette.disco;
                    totalRockRevenue += cassette.rock;
                    totalMetalRevenue += cassette.metal;
                    break;
            }
        }

        currentMoney += ((totalHipHopRevenue / 100f * currentListeners.hipHop * totalRevenueModifier.hipHop)
                    + (totalDiscoRevenue / 100f * currentListeners.disco * totalRevenueModifier.disco)
                    + (totalRockRevenue / 100f * currentListeners.rock * totalRevenueModifier.rock)
                    + (totalMetalRevenue / 100f * currentListeners.metal * totalRevenueModifier.metal));

        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (totalHipHopListeners / 100f) * totalListenerModifer.hipHop);
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (totalDiscoListeners / 100f) * totalListenerModifer.disco);
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (totalRockListeners / 100f) * totalListenerModifer.rock);
        currentListeners.metal += Mathf.CeilToInt(currentListeners.metal * (totalMetalListeners / 100f) * totalListenerModifer.metal);
    }

    //Stara funkcja zostawiona na potrzeby nagrody FlatListenersBoost w MiniGameReward
    public void AddListeners(GenreValues listenerGrowthPrecentage)
    {
        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f) * totalListenerModifer.hipHop);
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f) * totalListenerModifer.disco);
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f) * totalListenerModifer.rock);
        currentListeners.metal += Mathf.CeilToInt(currentListeners.metal * (listenerGrowthPrecentage.metal / 100f) * totalListenerModifer.metal);
    }

    public RadioStation()
    {
        currentListeners.hipHop = startListeners;
        currentListeners.disco = startListeners;
        currentListeners.rock = startListeners;
        currentListeners.metal = startListeners;

        hourlyListenersModifier.hipHop = defaultHourlyAndDailyModifier;
        hourlyListenersModifier.disco = defaultHourlyAndDailyModifier;
        hourlyListenersModifier.rock = defaultHourlyAndDailyModifier;
        hourlyListenersModifier.metal = defaultHourlyAndDailyModifier;

        dailyListenersModifier.hipHop = defaultHourlyAndDailyModifier;
        dailyListenersModifier.disco = defaultHourlyAndDailyModifier;
        dailyListenersModifier.rock = defaultHourlyAndDailyModifier;
        dailyListenersModifier.metal = defaultHourlyAndDailyModifier;

        hourlyRevenueModifier.hipHop = defaultHourlyAndDailyModifier;
        hourlyRevenueModifier.disco = defaultHourlyAndDailyModifier;
        hourlyRevenueModifier.rock = defaultHourlyAndDailyModifier;
        hourlyRevenueModifier.metal = defaultHourlyAndDailyModifier;

        dailyRevenueModifier.hipHop = defaultHourlyAndDailyModifier;
        dailyRevenueModifier.disco = defaultHourlyAndDailyModifier;
        dailyRevenueModifier.rock = defaultHourlyAndDailyModifier;
        dailyRevenueModifier.metal = defaultHourlyAndDailyModifier;

        totalListenerModifer.rock = defaultModifier;
        totalListenerModifer.metal = defaultModifier;
        totalListenerModifer.disco = defaultModifier;
        totalListenerModifer.hipHop = defaultModifier;

        totalRevenueModifier.hipHop = defaultModifier;
        totalRevenueModifier.rock = defaultModifier;
        totalRevenueModifier.disco = defaultModifier;
        totalRevenueModifier.metal = defaultModifier;
    }

    public void SetHourlyListenersModifier(float hipHop, float disco, float rock, float metal)
    {
        hourlyListenersModifier.hipHop = hipHop;
        hourlyListenersModifier.disco = disco;
        hourlyListenersModifier.rock = rock;
        hourlyListenersModifier.metal = metal;

        UpdateTotalListenersModifiers();
    }

    public void SetHourlyRevenueModifier(float hipHop, float disco, float rock, float metal)
    {
        hourlyRevenueModifier.hipHop = hipHop;
        hourlyRevenueModifier.disco = disco;
        hourlyRevenueModifier.rock = rock;
        hourlyRevenueModifier.metal = metal;

        UpdateTotalRevenueModifiers();
    }

    public void SetDailyListenersModifier(float hipHop, float disco, float rock, float metal)
    {
        dailyListenersModifier.hipHop = hipHop;
        dailyListenersModifier.disco = disco;
        dailyListenersModifier.rock = rock;
        dailyListenersModifier.metal = metal;

        UpdateTotalListenersModifiers();
    }

    public void SetDailyRevenueModifier(float hipHop, float disco, float rock, float metal)
    {
        dailyRevenueModifier.hipHop = hipHop;
        dailyRevenueModifier.disco = disco;
        dailyRevenueModifier.rock = rock;
        dailyRevenueModifier.metal = metal;

        UpdateTotalRevenueModifiers();
    }


    public void AddHourlyListenersModifier(float hipHop, float disco, float rock, float metal)
    {
        hourlyListenersModifier.hipHop += hipHop;
        hourlyListenersModifier.disco += disco;
        hourlyListenersModifier.rock += rock;
        hourlyListenersModifier.metal += metal;

        UpdateTotalListenersModifiers();
    }

    public void AddHourlyRevenueModifier(float hipHop, float disco, float rock, float metal)
    {
        hourlyRevenueModifier.hipHop += hipHop;
        hourlyRevenueModifier.disco += disco;
        hourlyRevenueModifier.rock += rock;
        hourlyRevenueModifier.metal += metal;

        UpdateTotalRevenueModifiers();
    }

    public void AddDailyListenersModifier(float hipHop, float disco, float rock, float metal)
    {
        dailyListenersModifier.hipHop += hipHop;
        dailyListenersModifier.disco += disco;
        dailyListenersModifier.rock += rock;
        dailyListenersModifier.metal += metal;

        UpdateTotalListenersModifiers();
    }

    public void AddDailyRevenueModifier(float hipHop, float disco, float rock, float metal)
    {
        dailyRevenueModifier.hipHop += hipHop;
        dailyRevenueModifier.disco += disco;
        dailyRevenueModifier.rock += rock;
        dailyRevenueModifier.metal += metal;

        UpdateTotalRevenueModifiers();
    }

    private void UpdateTotalListenersModifiers()
    {
        totalListenerModifer.disco = defaultModifier + hourlyListenersModifier.disco + dailyListenersModifier.disco;
        totalListenerModifer.hipHop = defaultModifier + hourlyListenersModifier.hipHop + dailyListenersModifier.hipHop;
        totalListenerModifer.rock = defaultModifier + hourlyListenersModifier.rock + dailyListenersModifier.rock;
        totalListenerModifer.metal = defaultModifier + hourlyListenersModifier.metal + dailyListenersModifier.metal;
    }

    private void UpdateTotalRevenueModifiers()
    {
        totalRevenueModifier.disco = defaultModifier + hourlyRevenueModifier.disco + dailyRevenueModifier.disco;
        totalRevenueModifier.hipHop = defaultModifier + hourlyRevenueModifier.hipHop + dailyRevenueModifier.hipHop;
        totalRevenueModifier.rock = defaultModifier + hourlyRevenueModifier.rock + dailyRevenueModifier.rock;
        totalRevenueModifier.metal = defaultModifier + hourlyRevenueModifier.metal + dailyRevenueModifier.metal;
    }

    public void AddListeners(GenreValues listenerGrowthPrecentage, int timesUsed)
    {
        int timesUsedSquared = timesUsed * timesUsed;
        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f) * totalListenerModifer.hipHop * (1f / timesUsedSquared));
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f) * totalListenerModifer.disco * (1f / timesUsedSquared));
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f) * totalListenerModifer.rock * (1f / timesUsedSquared));
        currentListeners.metal += Mathf.CeilToInt(currentListeners.metal * (listenerGrowthPrecentage.metal / 100f) * totalListenerModifer.metal * (1f / timesUsedSquared));
    }

    //public void AddListeners(GenreValues listenerGrowthPrecentage)
    //{
    //    currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f) * totalListenerModifer.hipHop);
    //    currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f) * totalListenerModifer.disco);
    //    currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f) * totalListenerModifer.rock);
    //    currentListeners.metal += Mathf.CeilToInt(currentListeners.metal * (listenerGrowthPrecentage.metal / 100f) * totalListenerModifer.metal);
    //}

    public void AddRevenue(GenreValues revenuGain, int timesUsed)
    {
        int timesUsedSquared = timesUsed * timesUsed;
        currentMoney += ((revenuGain.hipHop / 100f * currentListeners.hipHop * totalRevenueModifier.hipHop)
            + (revenuGain.disco / 100f * currentListeners.disco * totalRevenueModifier.disco)
            + (revenuGain.rock / 100f * currentListeners.rock * totalRevenueModifier.rock)
            + (revenuGain.metal / 100f * currentListeners.metal * totalRevenueModifier.metal)) * (1f / timesUsedSquared);
    }

    public float GetCurrentMoney()
    {
        return currentMoney;
    }

    public int GetTotalListeners()
    {
        return currentListeners.totalListeners;
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
    }
}
