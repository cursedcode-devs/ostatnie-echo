using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

public class Radio : MonoBehaviour
{
    private ChooseCassette chooseCassette;
    public GenreValues currentListeners;

    [SerializeField] private int startListeners = 5;
    [SerializeField] private int day = 0;
    [SerializeField] private int hour = 0;
    [SerializeField] private float currentMoney = 0.00f;
    private GameObject selectedObject;
    [SerializeField]
    private AudioSource source;
    private bool playing = false;

    public GameObject getSelectedObject() { return selectedObject; }

    public void AddListeners(GenreValues listenerGrowthPrecentage)
    {
        currentListeners.hipHop += currentListeners.hipHop * listenerGrowthPrecentage.hipHop;
        currentListeners.disco += currentListeners.disco * listenerGrowthPrecentage.disco;
        currentListeners.rock += currentListeners.rock * listenerGrowthPrecentage.rock;
        currentListeners.metal += currentListeners.metal * listenerGrowthPrecentage.metal;
    }


    public void AddRevenue(GenreValues revenuGain)
    {
        currentMoney += revenuGain.hipHop * currentListeners.hipHop
            + revenuGain.disco * currentListeners.disco
            + revenuGain.rock * currentListeners.rock
            + revenuGain.metal * currentListeners.metal;
    }

    void Start()
    {
        chooseCassette = GetComponent<ChooseCassette>();

        selectedObject = null;
        currentListeners.hipHop = startListeners;
        currentListeners.disco = startListeners;
        currentListeners.rock = startListeners;
        currentListeners.metal = startListeners;
    }

    // Update is called once per frame
    void Update()
    {
        MouseOperations();
        PlayMusic();
    }


    void MouseOperations()
    {
        Vector3 mousePosition;
        mousePosition = Mouse.current.position.ReadValue();
        bool leftMousePressed = Mouse.current.leftButton.wasPressedThisFrame;

        if (leftMousePressed)
        {
            if (selectedObject != null)
            {
                PlayingCassette(mousePosition);
            }
            SelectingCassettes(mousePosition);
        }
    }

    void PlayingCassette(Vector3 mousePosition)
    {
        GameObject clickedObject = chooseCassette.SelectingObject(mousePosition);
        if (selectedObject != null && clickedObject != null)  //Klikniêto na obiekt z colliderem gdy jest wybrana kasety
        {
            CassetteObject cassetteInfo = clickedObject.GetComponent<CassetteObject>();
            if (cassetteInfo == null)
            {
                Debug.Log("Klikniêto na obiekt z colliderem gdy jest wybrana kaseta. Puszczam muzyke");
                Debug.Log("TEST!!!!");
                if (!playing)
                {
                    source.Play();
                    playing = true;
                }
                else
                {
                    source.Stop();
                    playing = false;
                }

            }
        }
    }

    void SelectingCassettes(Vector3 mousePosition)
    {
        GameObject clickedObject = chooseCassette.SelectingObject(mousePosition);

        if (clickedObject != selectedObject && selectedObject != null && clickedObject != null)  //Klikniêto na obiekt z colliderem gdy jest wybrana kasety
        {
            Debug.Log("Klikniêto na obiekt z colliderem gdy jest wybrana kasety");
            selectedObject.transform.position += new Vector3(0f, -0.3f, 0f);
            selectedObject = clickedObject;

            CassetteObject cassetteInfo = selectedObject.GetComponent<CassetteObject>();
            if (cassetteInfo != null && cassetteInfo.data != null)
            {
                selectedObject = clickedObject;
                selectedObject.transform.position += new Vector3(0f, 0.3f, 0f);

                source.Stop();
                playing = false;
                source.clip = cassetteInfo.data.song;
            }
            else
            {
                // Klikniêto obiekt bez skryptu CassetteObject
                selectedObject = null;
            }

        }
        else if (clickedObject != selectedObject && selectedObject == null) //Klikniêto na obiekt z colliderem gdy nie ma wybranej kasety
        {
            Debug.Log("Klikniêto na obiekt z colliderem gdy nie ma wybranej kasety");
            selectedObject = clickedObject;

            CassetteObject cassetteInfo = selectedObject.GetComponent<CassetteObject>();
            if (cassetteInfo != null && cassetteInfo.data != null)
            {
                selectedObject = clickedObject;
                selectedObject.transform.position += new Vector3(0f, 0.3f, 0f);

                source.Stop();
                playing = false;
                source.clip = cassetteInfo.data.song;
            }
            else
            {
                selectedObject = null;
            }

        }
        else if (clickedObject == selectedObject && selectedObject != null) //Klikniêto na wybran¹ kasete 
        {
            Debug.Log("Klikniêto na wybran¹ kasete ");
            selectedObject.transform.position += new Vector3(0f, -0.3f, 0f);
            selectedObject = null;
        }
        else if (clickedObject == null && selectedObject != null)   //Klikniêto poza gdy jest wybrana kaseta
        {
            Debug.Log("Klikniêto poza gdy jest wybrana kaseta");
            selectedObject.transform.position += new Vector3(0f, -0.3f, 0f);
            selectedObject = null;
        }
    }


    void PlayMusic()
    {
        // Sprawdzamy czy spacja wciœniêta ORAZ czy mamy przypisane Ÿród³o dŸwiêku
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (source != null && playing == false)
            {
                Debug.Log("SPACJA! Gram dŸwiêk.");
                source.Play();
                playing = true;
            }
            else if (source != null && playing == true)
            {
                source.Stop();
                playing = false;
            }
            else
            {
                Debug.LogError("Zapomnia³eœ przypisaæ AudioSource do skryptu w inspektorze!");
            }
        }
    }

}
