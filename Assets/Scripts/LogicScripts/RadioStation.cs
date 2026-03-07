using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

/// <summary>
/// Klasa zajmuj�ca si� prezchowywanie stanu radio i zmienianiem go
/// </summary>
[System.Serializable]
public class RadioStation
{
    public GenreValues currentListeners;

    [SerializeField] private int startListeners = 5;
    [SerializeField] private float currentMoney = 0.00f;
    [SerializeField] private GenreValuesModifier listenersModifier;
    [SerializeField] private GenreValuesModifier revenueModifier;

    public RadioStation() 
    {
        currentListeners.hipHop = startListeners;
        currentListeners.disco = startListeners;
        currentListeners.rock = startListeners;
        currentListeners.metal = startListeners;
    }

    public void setListenersModifier(float hipHop, float disco, float rock, float metal)
    {
        listenersModifier.hipHop = hipHop;
        listenersModifier.disco = disco;
        listenersModifier.rock = rock;
        listenersModifier.metal = metal;
    }

    public void setRevenueModifier(float hipHop, float disco, float rock, float metal)
    {
        revenueModifier.hipHop = hipHop;
        revenueModifier.disco = disco;
        revenueModifier.rock = rock;
        revenueModifier.metal = metal;
    }

    public void AddListeners(GenreValues listenerGrowthPrecentage)
    {
        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f) * listenersModifier.hipHop);
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f) * listenersModifier.disco);
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f) * listenersModifier.rock);
        currentListeners.metal += Mathf.CeilToInt(currentListeners.metal * (listenerGrowthPrecentage.metal / 100f) * listenersModifier.metal);
    }

    public void AddRevenue(GenreValues revenuGain)
    {
        currentMoney += (revenuGain.hipHop / 100f * currentListeners.hipHop * revenueModifier.hipHop)
            + (revenuGain.disco / 100f * currentListeners.disco * revenueModifier.disco)
            + (revenuGain.rock / 100f * currentListeners.rock * revenueModifier.rock)
            + (revenuGain.metal / 100f * currentListeners.metal * revenueModifier.metal);
    }
    
    public float GetCurrentMoney()
    {
        return currentMoney;
    }

}
