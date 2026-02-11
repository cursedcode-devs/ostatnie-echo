using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Radio : MonoBehaviour
{
    [SerializeField]
    private int day = 0;
    [SerializeField]
    private int hour = 0;
    [SerializeField]
    private int listenersTotal = 0;
    [SerializeField]
    private int listenersHipHop = 0;
    [SerializeField]
    private int listenersDisco = 0;
    [SerializeField]
    private int listenersRock = 0;
    [SerializeField]
    private int listenersMetal = 0;
    [SerializeField]
    private int money = 0;


    private List<GameObject> ownedCassettes = new List<GameObject>();
    private List<GameObject> ownedAds = new List<GameObject>();
    private List<GameObject> ownedImprovements = new List<GameObject>();    //leszpa antena, kawa itd. Nie mogê wpaœæ na lepsz¹ nazwe


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        
    }

}
