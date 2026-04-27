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
    [SerializeReference] public ObjectSelectionHandler selectionHandler;
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
    public GameObject choosingCassetteUI;
    private bool choosingCassette = false;

    void Start()
    {
        actionManager = new ActionManager(mainCamera);
        radioStation = new RadioStation();
        timeHandler = new TimeHandler(startHour, startDay);
        selectionHandler = new ObjectSelectionHandler();
        

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

        if (clickedObject != null)
            sliderObject = clickedObject.GetComponent<ConsoleSliderObject>();

        //Obs�uguje akcej myszki
        switch (mouseActionType)
        {
            case ActionTypes.LeftClickOnPlayableObject:
                Debug.Log("GetActionType - LeftClickOnPlayableObject");
                FMODUnity.RuntimeManager.PlayOneShot(clickSound, this.transform.position);
                selectionHandler.SelectObject(clickedObject);
                break;
            case ActionTypes.LeftClickOnPlayingObject:
                Debug.Log("GetActionType - LeftClickOnPlayingObject");
                
                if (selectionHandler.IsSelectedObjectPlayable() || playing)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(putCasetteInSound, this.transform.position);
                    PlayPlayableObject(clickedObject);
                }
                else
                {
                    selectionHandler.SelectObject(clickedObject);
                }
                choosingCassette = !choosingCassette;
                choosingCassetteUI.SetActive(choosingCassette);
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
            case ActionTypes.LeftClickOutsiedObject:
                Debug.Log("GetActionType - LeftClickOutsiedObject");
                if (selectionHandler.GetSelectedObject() != null)
                    selectionHandler.DeselectedObject();
                break;
        }

        keyboardActionType = actionManager.GetKeyboardActionType();
        //Obs�uguje akcje klawiatury
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
                radioStation.ApplySegment(airtime.GetCassettes());
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

private void PlayPlayableObject(GameObject clickedObject)
    {

        //if (playing)
        //{
        //    StopPlayingAudio();
        //    return;
        //}

        //if (selectionHandler.GetSelectedObject() == null)
        //{
        //    return;
        //}

        

        //PlayableObject playableObject = selectionHandler.GetSelectedObject().GetComponent<PlayableObject>();

        //if (playableObject == null) return;

        //if (playableObject.data == null) return;

        //playableObject.data.Play(ref source);
        //playableObject.data.ApplyEffect(radioStation);
        //playing = true;
        //MiniGameSystem.Instance.LaunchRandom();
        //StartCoroutine(WaitForAudioToEnd());

    }

    private IEnumerator WaitForAudioToEnd()
    {
        while (source.isPlaying)
        {
            yield return null;
        }

        playing = false;

        if (timeHandler != null)
        {
            timeHandler.NextHour();
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
    private void StopPlayingAudio()
    {
        source.Stop();
        playing = false;
    }

}