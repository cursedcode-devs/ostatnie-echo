using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

/// <summary>
/// Klasa zajmuj¹ca siê prezchowywanie stanu radio i zmienianiem go
/// </summary>
public class RadioStation : MonoBehaviour
{
    public GenreValues currentListeners;

    [SerializeField] private int startListeners = 5;
    //[SerializeField] private int day = 0;
    //[SerializeField] private int hour = 0;
    [SerializeField] private float currentMoney = 0.00f;

    public void AddListeners(GenreValues listenerGrowthPrecentage)
    {
        currentListeners.hipHop += Mathf.CeilToInt(currentListeners.hipHop * (listenerGrowthPrecentage.hipHop / 100f));
        currentListeners.disco += Mathf.CeilToInt(currentListeners.disco * (listenerGrowthPrecentage.disco / 100f));
        currentListeners.rock += Mathf.CeilToInt(currentListeners.rock * (listenerGrowthPrecentage.rock / 100f));
        currentListeners.metal += Mathf.CeilToInt(currentListeners.metal * (listenerGrowthPrecentage.metal / 100f));
    }


    public void AddRevenue(GenreValues revenuGain)
    {
        currentMoney += revenuGain.hipHop / 100f * currentListeners.hipHop
            + revenuGain.disco / 100f * currentListeners.disco
            + revenuGain.rock / 100f * currentListeners.rock
            + revenuGain.metal / 100f * currentListeners.metal;
    }

    void Start()
    {
        currentListeners.hipHop = startListeners;
        currentListeners.disco = startListeners;
        currentListeners.rock = startListeners;
        currentListeners.metal = startListeners;
    }

    // Update is called once per frame
    void Update()
    {

    }

}
