using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public RadioStation radioStation;
    [SerializeField] private AudioSource source;
    [SerializeField] private Camera mainCamera;
    private bool playing = false;
    private ActionTypes mouseActionType;
    private ActionTypes keyboardActionType;
    public ActionManager actionManager;
    public TimeHandler timeHandler;
    private StatsUI statsUI;
    private Vector3 addedObjectRotation = Vector3.zero;
    private float rotationSpeed = 3f;
    public ObjectSelectionHandler selectionHandler;
    public ConsoleSliderObject amplitudeSlider;
    public ConsoleSliderObject lengthSlider;
    public ConsoleSliderObject frequencySlider;
    public FMODUnity.EventReference clickSound;
    public FMODUnity.EventReference putCasetteInSound;
    private int startHour = 14;
    private int startDay = 1;
    private const float startingModifier = 0f;

    private bool inputEnabled = false;

    //[SerializeField] private List<PlayableContent> ownedCassettes = new List<PlayableContent>();
    //[SerializeField] private List<PlayableContent> selectedCassettes = new List<PlayableContent>();

    public Airtime airtime;
    public ChoosingCassetteUI choosingCassetteUI;
    public AudioQueueManager audioQueueManager;
    public CassetteSlotHandler[] cassetteSlots;

    void Start()
    {
        actionManager = new ActionManager(mainCamera);
        radioStation = new RadioStation();
        timeHandler = new TimeHandler(startHour, startDay, airtime, cassetteSlots);

        // Auto-create AdContractManager component so it exists in the scene
        gameObject.AddComponent<AdContractManager>();

        audioQueueManager.SetTimeHandler(timeHandler);

        radioStation.SetHourlyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetDailyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetHourlyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetDailyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);

        DayEndHandler dayEndHandler = FindFirstObjectByType<DayEndHandler>();
        if (dayEndHandler != null)
        {
            dayEndHandler.Initialize(radioStation, timeHandler, this);
        }
        StatsUI statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
        {
            statsUI.Initialize(radioStation);
        }
        var miniGameSystem = FindFirstObjectByType<MiniGameSystem>();
    }

    void Update()
    {
        if (!inputEnabled) return;

        mouseActionType = actionManager.GetActionMouseType();
        GameObject clickedObject = actionManager.GetPointedObject();
        ConsoleSliderObject sliderObject = null;
        CassetteSlotHandler slotHandler = null;

        if (clickedObject != null)
            sliderObject = clickedObject.GetComponent<ConsoleSliderObject>();

        //Obsługuje akcej myszki
        switch (mouseActionType)
        {
            case ActionTypes.LeftClickOnPlayableObject:
                Debug.Log("GetActionType - LeftClickOnPlayableObject");
                FMODUnity.RuntimeManager.PlayOneShot(clickSound, this.transform.position);
                selectionHandler.SelectObject(clickedObject);
                break;
            case ActionTypes.LeftClickOnPlayingObject:
                Debug.Log("GetActionType - LeftClickOnPlayingObject");

                if (selectionHandler.IsObjectPlayable() || playing)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(putCasetteInSound, this.transform.position);
                }
                else
                {
                    //Na razie nie da się selectować odtwarzacza, bo to robi problemy z selectowanie kaset w slotach. 
                    //Jak będzie czas coś można przykminić

                    //selectionHandler.SelectObject(clickedObject);
                }
                choosingCassetteUI.ToggleVisibility(audioQueueManager.IsPlaying());
                break;
            case ActionTypes.LeftClickOnObject:
                Debug.Log("GetActionType - LeftClickOnObject");
                FMODUnity.RuntimeManager.PlayOneShot(clickSound, this.transform.position);
                selectionHandler.SelectObject(clickedObject);
                break;
            case ActionTypes.LeftClickOnSlider:
                Debug.Log("GetActionType - LeftClickOnSlider");
                if (sliderObject != null)
                    sliderObject.OnMouseClick();
                break;
            case ActionTypes.LeftPressedOnSlider:
                Debug.Log("GetActionType - LeftPressedOnSlider");
                if (sliderObject != null)
                    sliderObject.OnMousePressed();
                break;
            case ActionTypes.LeftClickOnSlotHinge:
                Debug.Log("GetActionType - LeftClickOnSlot");
                //selectionHandler.SelectObject(clickedObject);
                slotHandler = clickedObject.GetComponent<CassetteSlotHandler>();
                slotHandler.HandleHinge();
                break;
            case ActionTypes.LeftClickOnSlotHitBox:
                Debug.Log("GetActionType - LeftClickOnSlotHitBox");
                slotHandler = clickedObject.GetComponent<SlotHitBox>().GetSlotHandler();

                if (selectionHandler.GetSelectedObject() == null)
                {
                    slotHandler.PutCassetteOut();
                    break;
                } 

                if (selectionHandler.GetSelectedObject().CompareTag("Playable"))
                {
                    GameObject selectedObject = selectionHandler.GetSelectedObject();
                    if(slotHandler.PutCassetteIn(selectedObject))
                        selectionHandler.DeselectedObject(false, false);
                }
                break;
            case ActionTypes.LeftClickOnShelf:
                if (selectionHandler.GetSelectedObject() == null)
                    break;
                if (selectionHandler.GetSelectedObject().CompareTag("Playable"))
                    selectionHandler.DeselectedObject();
                break;
            case ActionTypes.LeftClickOutsiedObject:
                Debug.Log("GetActionType - LeftClickOutsiedObject");
                if (selectionHandler.GetSelectedObject() != null)
                    selectionHandler.DeselectedObject();
                break;
        }

        keyboardActionType = actionManager.GetKeyboardActionType();
        //Obsługuje akcje klawiatury
        switch (keyboardActionType)
        {
            case ActionTypes.PressedA:
                Debug.Log("Wcisnieto A");
                addedObjectRotation = new Vector3(0f, 0f, -rotationSpeed);
                break;
            case ActionTypes.PressedD:
                Debug.Log("Wcisnieto D");
                addedObjectRotation = new Vector3(0f, 0f, rotationSpeed);
                break;
            case ActionTypes.PressedW:
                Debug.Log("Wcisnieto W");
                addedObjectRotation = new Vector3(-rotationSpeed, 0f, 0f);
                break;
            case ActionTypes.PressedS:
                Debug.Log("Wcisnieto S");
                addedObjectRotation = new Vector3(rotationSpeed, 0f, 0f);
                break;
            case ActionTypes.PressedQ:
                Debug.Log("Wcisnieto Q");
                addedObjectRotation = new Vector3(0f, rotationSpeed, 0f);
                break;
            case ActionTypes.PressedE:
                Debug.Log("Wcisnieto E");
                addedObjectRotation = new Vector3(0f, -rotationSpeed, 0f);
                break;
            case ActionTypes.PressedEnter:
                Debug.Log("Wcisnieto Enter");
                PlayableContent[] playedCassettes = airtime.GetCassettes();
                radioStation.ApplySegment(playedCassettes);
                audioQueueManager.EnqueueClips(airtime.GetCassettesAudio());
                audioQueueManager.PlayClipsSequence();
                CheckForRequestedCassette();

                // Destroy physical ad cassettes that were just played
                var adManager = FindFirstObjectByType<AdContractManager>();
                if (adManager != null)
                {
                    adManager.HandleAdsPlayed(playedCassettes, cassetteSlots);
                }

                choosingCassetteUI.UpdatePredictions();
                choosingCassetteUI.Hide();
                break;
            case ActionTypes.PressedP:
                audioQueueManager.SkipSong();
                break;
            case ActionTypes.None:
                addedObjectRotation = Vector3.zero;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (addedObjectRotation != Vector3.zero)
            selectionHandler.RotateObject(addedObjectRotation.x, addedObjectRotation.y, addedObjectRotation.z);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public string requestedGenre = "";
    public PlayableContent requestedCassette;
    public float requestedCassetteBoost = 0.1f; // 10% boost

    public void SetRequestedCassette(PlayableContent cassette)
    {
        requestedCassette = cassette;
    }

    public void SetRequestedGenre(string genre)
    {
        requestedGenre = genre;
    }


    public void CheckForRequestedCassette()
    {
        bool wasPlayed = false;
        string successfullyPlayedGenre = "";

        if (requestedCassette != null)
        {
            foreach (var cassette in airtime.GetCassettes())
            {
                if (cassette == requestedCassette)
                {
                    wasPlayed = true;
                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(requestedGenre))
        {
            foreach (var cassette in airtime.GetCassettes())
            {
                if (cassette != null)
                {
                    GenreValues values = cassette.GetCassetteValues();
                    float genreValue = 0;
                    switch (requestedGenre.ToLower())
                    {
                        case "hiphop":
                        case "hip hop": genreValue = values.hipHop; break;
                        case "disco": genreValue = values.disco; break;
                        case "rock": genreValue = values.rock; break;
                        case "pop": genreValue = values.pop; break;
                    }

                    if (genreValue > 0)
                    {
                        wasPlayed = true;
                        successfullyPlayedGenre = requestedGenre.ToLower();
                        break;
                    }
                }
            }
        }

        if (wasPlayed)
        {
            float boostHipHop = 0f, boostDisco = 0f, boostRock = 0f, boostMetal = 0f;

            if (successfullyPlayedGenre == "hiphop" || successfullyPlayedGenre == "hip hop") boostHipHop = requestedCassetteBoost;
            else if (successfullyPlayedGenre == "disco") boostDisco = requestedCassetteBoost;
            else if (successfullyPlayedGenre == "rock") boostRock = requestedCassetteBoost;
            else if (successfullyPlayedGenre == "pop") boostMetal = requestedCassetteBoost;
            else
            {
                // Fallback for specific cassette request without genre specified
                boostHipHop = requestedCassetteBoost;
                boostDisco = requestedCassetteBoost;
                boostRock = requestedCassetteBoost;
                boostMetal = requestedCassetteBoost;
            }

            Debug.Log("Zagrałeś pożądaną piosenkę/gatunek! Boost do słuchaczy!");
            radioStation.AddHourlyListenersModifier(boostHipHop, boostDisco, boostRock, boostMetal);

            if (MiniGameSystem.Instance != null)
            {
                MiniGameSystem.Instance.ShowPopup("Sukces", "Zagrałeś pożądany gatunek!");
            }
        }

        requestedCassette = null;
        requestedGenre = "";
    }
}