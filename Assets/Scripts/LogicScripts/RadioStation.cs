using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa zajmuj�ca si� prezchowywanie stanu radio i zmienianiem go
/// </summary>
[System.Serializable]
public class RadioStation
{
    public GenreValues currentListeners;

    [SerializeField] private int startListeners;
    [SerializeField] private float currentMoney;
    [SerializeField] private GenreValuesModifier hourlyListenersModifier;
    [SerializeField] private GenreValuesModifier hourlyRevenueModifier;
    [SerializeField] private GenreValuesModifier dailyListenersModifier;
    [SerializeField] private GenreValuesModifier dailyRevenueModifier;
    private float defaultModifier = 1f;
    private float defaultHourlyAndDailyModifier = 0f;
    [SerializeField] private GenreValuesModifier totalListenerModifer;
    [SerializeField] private GenreValuesModifier totalRevenueModifier;

    public void ApplySegment(PlayableContent[] cassettes)
    {

        SegmentChangeVal changeVal = CalculateSegment(cassettes);


        currentMoney += changeVal.money;
        currentListeners.hipHop += changeVal.hipHop;
        currentListeners.disco += changeVal.disco;
        currentListeners.rock += changeVal.rock;
        currentListeners.pop += changeVal.pop;

        if(currentListeners.hipHop < 0)
            currentListeners.hipHop = 0;
        if (currentListeners.disco < 0)
            currentListeners.disco = 0;
        if (currentListeners.rock < 0)
            currentListeners.rock = 0;
        if (currentListeners.pop < 0)
            currentListeners.pop = 0;
    }

    public SegmentChangeVal CalculateSegment(PlayableContent[] cassettes)
    {
        SegmentChangeVal val = new SegmentChangeVal();


        val.hipHop = 0;
        val.disco = 0;
        val.rock = 0;
        val.pop = 0;
        val.money = 0f;

        int totalHipHopListeners = 0;
        int totalDiscoListeners = 0;
        int totalRockListeners = 0;
        int totalPopListeners = 0;

        int totalHipHopRevenue = 0;
        int totalDiscoRevenue = 0;
        int totalRockRevenue = 0;
        int totalPopRevenue = 0;

        int adsPlayed = 0;

        for (int i = 0; i < cassettes.Length; i++)
        {
            if (cassettes[i] == null)
                continue;

            switch (cassettes[i].GetType())
            {
                case CassetteTypes.Music:
                    if (cassettes[i].GetTimesUsed() == 0)
                    {
                        totalHipHopListeners += cassettes[i].GetCassetteValues().hipHop;
                        totalDiscoListeners += cassettes[i].GetCassetteValues().disco;
                        totalRockListeners += cassettes[i].GetCassetteValues().rock;
                        totalPopListeners += cassettes[i].GetCassetteValues().pop;
                    }
                    else
                    {
                        Debug.Log("TimesUsed: " + cassettes[i].GetTimesUsed());
                        int hipHop = cassettes[i].GetLastValues().hipHop / 2;
                        int disco = cassettes[i].GetLastValues().disco / 2;
                        int rock = cassettes[i].GetLastValues().rock / 2;
                        int pop = cassettes[i].GetLastValues().pop / 2;

                        hipHop = cassettes[i].GetLastValues().hipHop - Mathf.Abs(hipHop);
                        disco = cassettes[i].GetLastValues().disco - Mathf.Abs(disco);
                        rock = cassettes[i].GetLastValues().rock - Mathf.Abs(rock);
                        pop = cassettes[i].GetLastValues().pop - Mathf.Abs(pop);

                        totalHipHopListeners += hipHop;
                        totalDiscoListeners += disco;
                        totalRockListeners += rock;
                        totalPopListeners += pop;

                        cassettes[i].SetLastValues(hipHop, disco, rock, pop);
                    }
                    // Back2Back: jeden slot na godzinę mnoży swój wkład x2
                    if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryConsumeBack2Back())
                    {
                        if (cassettes[i].GetTimesUsed() == 0)
                        {
                            totalHipHopListeners += cassettes[i].GetCassetteValues().hipHop;
                            totalDiscoListeners  += cassettes[i].GetCassetteValues().disco;
                            totalRockListeners   += cassettes[i].GetCassetteValues().rock;
                            totalPopListeners    += cassettes[i].GetCassetteValues().pop;
                        }
                        else
                        {
                            int lastHipHop = cassettes[i].GetLastValues().hipHop - Mathf.Abs(cassettes[i].GetLastValues().hipHop / 2);
                            int lastDisco  = cassettes[i].GetLastValues().disco  - Mathf.Abs(cassettes[i].GetLastValues().disco  / 2);
                            int lastRock   = cassettes[i].GetLastValues().rock   - Mathf.Abs(cassettes[i].GetLastValues().rock   / 2);
                            int lastPop    = cassettes[i].GetLastValues().pop    - Mathf.Abs(cassettes[i].GetLastValues().pop    / 2);

                            totalHipHopListeners += lastHipHop;
                            totalDiscoListeners  += lastDisco;
                            totalRockListeners   += lastRock;
                            totalPopListeners    += lastPop;
                        }
                        Debug.Log("[Back2Back] Podwojono wkład slotu!");
                    }
                    cassettes[i].IncreaseTimesUsed();
                    break;
                //W takim wypadku rekalmy nie potrzebują zmiennej timesUsed
                case CassetteTypes.Ad:
                    adsPlayed++;
                    totalHipHopRevenue += cassettes[i].GetCassetteValues().hipHop;
                    totalDiscoRevenue += cassettes[i].GetCassetteValues().disco;
                    totalRockRevenue += cassettes[i].GetCassetteValues().rock;
                    totalPopRevenue += cassettes[i].GetCassetteValues().pop;
                    cassettes[i].IncreaseTimesUsed();
                    break;
            }
        }

        val.money += ((totalHipHopRevenue / 100f * currentListeners.hipHop * totalRevenueModifier.hipHop)
                    + (totalDiscoRevenue / 100f * currentListeners.disco * totalRevenueModifier.disco)
                    + (totalRockRevenue / 100f * currentListeners.rock * totalRevenueModifier.rock)
                    + (totalPopRevenue / 100f * currentListeners.pop * totalRevenueModifier.pop));

        AdsPunishment(adsPlayed);

        val.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (totalHipHopListeners / 100f) * totalListenerModifer.hipHop);
        val.disco += Mathf.CeilToInt(currentListeners.disco * (totalDiscoListeners / 100f) * totalListenerModifer.disco);
        val.rock += Mathf.CeilToInt(currentListeners.rock * (totalRockListeners / 100f) * totalListenerModifer.rock);
        val.pop += Mathf.CeilToInt(currentListeners.pop * (totalPopListeners / 100f) * totalListenerModifer.pop);

        return val;
    }

    public void AdsPunishment(int adsPlayed)
    {
        if (adsPlayed < 1)
            return;

        if (adsPlayed == 1)
        {
            AddHourlyListenersModifier(-0.05f, -0.05f, -0.05f, -0.05f);
        }
        else if (adsPlayed == 2)
        {
            AddHourlyListenersModifier(-0.07f, -0.07f, -0.07f, -0.07f);
            AddDailyListenersModifier(-0.05f, -0.05f, -0.05f, -0.05f);
            RemoveListenersPr(0.05f);
        }
        else if (adsPlayed >= 3)
        {
            AddHourlyListenersModifier(-0.1f, -0.1f, -0.1f, -0.1f);
            AddDailyListenersModifier(-0.07f, -0.07f, -0.07f, -0.07f);
            RemoveListenersPr(0.1f);
        }
    }

    public void RemoveListenersPr(float precentage)
    {
        currentListeners.hipHop -= Mathf.CeilToInt(currentListeners.hipHop * precentage);
        currentListeners.disco -= Mathf.CeilToInt(currentListeners.disco * precentage);
        currentListeners.rock -= Mathf.CeilToInt(currentListeners.rock * precentage);
        currentListeners.pop -= Mathf.CeilToInt(currentListeners.pop * precentage);
    }

    //Stara funkcja zostawiona na potrzeby nagrody FlatListenersBoost w MiniGameReward
    public void AddListeners(GenreValues listenerGrowthPrecentage)
    {
        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f) * totalListenerModifer.hipHop);
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f) * totalListenerModifer.disco);
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f) * totalListenerModifer.rock);
        currentListeners.pop += Mathf.CeilToInt(currentListeners.pop * (listenerGrowthPrecentage.pop / 100f) * totalListenerModifer.pop);
    }

    public RadioStation()
    {
        currentListeners.hipHop = startListeners;
        currentListeners.disco = startListeners;
        currentListeners.rock = startListeners;
        currentListeners.pop = startListeners;

        hourlyListenersModifier.hipHop = defaultHourlyAndDailyModifier;
        hourlyListenersModifier.disco = defaultHourlyAndDailyModifier;
        hourlyListenersModifier.rock = defaultHourlyAndDailyModifier;
        hourlyListenersModifier.pop = defaultHourlyAndDailyModifier;

        dailyListenersModifier.hipHop = defaultHourlyAndDailyModifier;
        dailyListenersModifier.disco = defaultHourlyAndDailyModifier;
        dailyListenersModifier.rock = defaultHourlyAndDailyModifier;
        dailyListenersModifier.pop = defaultHourlyAndDailyModifier;

        hourlyRevenueModifier.hipHop = defaultHourlyAndDailyModifier;
        hourlyRevenueModifier.disco = defaultHourlyAndDailyModifier;
        hourlyRevenueModifier.rock = defaultHourlyAndDailyModifier;
        hourlyRevenueModifier.pop = defaultHourlyAndDailyModifier;

        dailyRevenueModifier.hipHop = defaultHourlyAndDailyModifier;
        dailyRevenueModifier.disco = defaultHourlyAndDailyModifier;
        dailyRevenueModifier.rock = defaultHourlyAndDailyModifier;
        dailyRevenueModifier.pop = defaultHourlyAndDailyModifier;

        totalListenerModifer.rock = defaultModifier;
        totalListenerModifer.pop = defaultModifier;
        totalListenerModifer.disco = defaultModifier;
        totalListenerModifer.hipHop = defaultModifier;

        totalRevenueModifier.hipHop = defaultModifier;
        totalRevenueModifier.rock = defaultModifier;
        totalRevenueModifier.disco = defaultModifier;
        totalRevenueModifier.pop = defaultModifier;
    }

    public void SetHourlyListenersModifier(float hipHop, float disco, float rock, float pop)
    {
        hourlyListenersModifier.hipHop = hipHop;
        hourlyListenersModifier.disco = disco;
        hourlyListenersModifier.rock = rock;
        hourlyListenersModifier.pop = pop;

        UpdateTotalListenersModifiers();
    }

    public void SetHourlyRevenueModifier(float hipHop, float disco, float rock, float pop)
    {
        hourlyRevenueModifier.hipHop = hipHop;
        hourlyRevenueModifier.disco = disco;
        hourlyRevenueModifier.rock = rock;
        hourlyRevenueModifier.pop = pop;

        UpdateTotalRevenueModifiers();
    }

    public void SetDailyListenersModifier(float hipHop, float disco, float rock, float pop)
    {
        dailyListenersModifier.hipHop = hipHop;
        dailyListenersModifier.disco = disco;
        dailyListenersModifier.rock = rock;
        dailyListenersModifier.pop = pop;

        UpdateTotalListenersModifiers();
    }

    public void SetDailyRevenueModifier(float hipHop, float disco, float rock, float pop)
    {
        dailyRevenueModifier.hipHop = hipHop;
        dailyRevenueModifier.disco = disco;
        dailyRevenueModifier.rock = rock;
        dailyRevenueModifier.pop = pop;

        UpdateTotalRevenueModifiers();
    }


    public void AddHourlyListenersModifier(float hipHop, float disco, float rock, float pop)
    {
        hourlyListenersModifier.hipHop += hipHop;
        hourlyListenersModifier.disco += disco;
        hourlyListenersModifier.rock += rock;
        hourlyListenersModifier.pop += pop;

        UpdateTotalListenersModifiers();
    }

    public void AddHourlyRevenueModifier(float hipHop, float disco, float rock, float pop)
    {
        hourlyRevenueModifier.hipHop += hipHop;
        hourlyRevenueModifier.disco += disco;
        hourlyRevenueModifier.rock += rock;
        hourlyRevenueModifier.pop += pop;

        UpdateTotalRevenueModifiers();
    }

    public void AddDailyListenersModifier(float hipHop, float disco, float rock, float pop)
    {
        dailyListenersModifier.hipHop += hipHop;
        dailyListenersModifier.disco += disco;
        dailyListenersModifier.rock += rock;
        dailyListenersModifier.pop += pop;

        UpdateTotalListenersModifiers();
    }

    public void AddDailyRevenueModifier(float hipHop, float disco, float rock, float pop)
    {
        dailyRevenueModifier.hipHop += hipHop;
        dailyRevenueModifier.disco += disco;
        dailyRevenueModifier.rock += rock;
        dailyRevenueModifier.pop += pop;

        UpdateTotalRevenueModifiers();
    }

    private void UpdateTotalListenersModifiers()
    {
        totalListenerModifer.disco = defaultModifier + hourlyListenersModifier.disco + dailyListenersModifier.disco;
        totalListenerModifer.hipHop = defaultModifier + hourlyListenersModifier.hipHop + dailyListenersModifier.hipHop;
        totalListenerModifer.rock = defaultModifier + hourlyListenersModifier.rock + dailyListenersModifier.rock;
        totalListenerModifer.pop = defaultModifier + hourlyListenersModifier.pop + dailyListenersModifier.pop;
    }

    private void UpdateTotalRevenueModifiers()
    {
        totalRevenueModifier.disco = defaultModifier + hourlyRevenueModifier.disco + dailyRevenueModifier.disco;
        totalRevenueModifier.hipHop = defaultModifier + hourlyRevenueModifier.hipHop + dailyRevenueModifier.hipHop;
        totalRevenueModifier.rock = defaultModifier + hourlyRevenueModifier.rock + dailyRevenueModifier.rock;
        totalRevenueModifier.pop = defaultModifier + hourlyRevenueModifier.pop + dailyRevenueModifier.pop;
    }

    public void AddListeners(GenreValues listenerGrowthPrecentage, int timesUsed)
    {
        int timesUsedSquared = timesUsed * timesUsed;
        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f) * totalListenerModifer.hipHop * (1f / timesUsedSquared));
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f) * totalListenerModifer.disco * (1f / timesUsedSquared));
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f) * totalListenerModifer.rock * (1f / timesUsedSquared));
        currentListeners.pop += Mathf.CeilToInt(currentListeners.pop * (listenerGrowthPrecentage.pop / 100f) * totalListenerModifer.pop * (1f / timesUsedSquared));
    }

    public void AddRevenue(GenreValues revenuGain, int timesUsed)
    {
        int timesUsedSquared = timesUsed * timesUsed;
        currentMoney += ((revenuGain.hipHop / 100f * currentListeners.hipHop * totalRevenueModifier.hipHop)
            + (revenuGain.disco / 100f * currentListeners.disco * totalRevenueModifier.disco)
            + (revenuGain.rock / 100f * currentListeners.rock * totalRevenueModifier.rock)
            + (revenuGain.pop / 100f * currentListeners.pop * totalRevenueModifier.pop)) * (1f / timesUsedSquared);
    }

    public float GetCurrentMoney()
    {
        return currentMoney;
    }

    public void SetCurrentMoney(float newAmountOfMoney)
    {
        currentMoney = newAmountOfMoney;
    }

    public int GetTotalListeners()
    {
        return currentListeners.totalListeners;
    }

    public GenreValuesModifier GetTotalListenerModifier()
    {
        return totalListenerModifer;
    }

    public GenreValuesModifier GetTotalRevenueModifier()
    {
        return totalRevenueModifier;
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
    }

}
