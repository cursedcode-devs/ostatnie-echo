using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public RadioStation radioStation;
    private GameObject selectedCassette;
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
    [SerializeReference] private ObjectSelectionHandler selectionHandler;
    public ConsoleSliderObject amplitudeSlider;
    public ConsoleSliderObject lengthSlider;
    public ConsoleSliderObject frequencySlider;

    public int amountOfListenersToWin = 500;
    [Header("Ekrany tekstowe")]
    public GameObject instructionCanvas;
    public GameObject gameFailCanvas;
    public GameObject gameWonCanvas;
    public GameObject FailMoneyText;
    public GameObject FailEndListenersText;

    private int startHour = 14;
    private int startDay = 1;
    private const float startingModifier = 0f;

    
    private bool inputEnabled = true;


    void Start()
    {
        gameFailCanvas.SetActive(false);
        gameWonCanvas.SetActive(false);
        instructionCanvas.SetActive(true);

        selectedCassette = null;

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
            dayEndHandler.Initialize(radioStation, timeHandler);
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
        if (instructionCanvas.activeInHierarchy)
            return;
        if (gameFailCanvas.activeInHierarchy)
        {
            ExitGame();
            return;
        }
        if (gameWonCanvas.activeInHierarchy)
        {
            ExitGame();
            return;
        }

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
                selectionHandler.SelectObject(clickedObject);
                break;
            case ActionTypes.LeftClickOnPlayingObject:
                Debug.Log("GetActionType - LeftClickOnPlayingObject");
                if (selectionHandler.IsSelectedObjectPlayable() || playing)
                {
                    PlayPlayableObject(clickedObject);
                }
                else
                {
                    selectionHandler.SelectObject(clickedObject);
                }
                break;
            case ActionTypes.LeftClickOnObject:
                Debug.Log("GetActionType - LeftClickOnObject");
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

        if (playing)
        {
            StopPlayingAudio();
            return;
        }

        if (selectionHandler.GetSelectedObject() == null)
        {
            return;
        }



        PlayableObject playableObject = selectionHandler.GetSelectedObject().GetComponent<PlayableObject>();

        if (playableObject == null) return;

        if (playableObject.data == null) return;

        playableObject.data.Play(ref source);
        playableObject.data.ApplyEffect(radioStation);


        playing = true;
        StartCoroutine(WaitForAudioToEnd());

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
            if (timeHandler.FinishGame())
            {
                if (radioStation.GetTotalListeners() < amountOfListenersToWin)
                    EndGame(2);
                else if (radioStation.GetCurrentMoney() <= 0)
                    EndGame(1);
                else EndGame(0);
            }
            else
            {
                if (radioStation.GetCurrentMoney() <= 0)
                    EndGame(1);
                else MiniGameSystem.Instance.LaunchRandom();
            }
        }
    }

    public void EndGame(int endReason)
    {
        switch (endReason)
        {
            //Wygrana
            case 0:
                gameWonCanvas.SetActive(true);
                break;
            //Zero lub mniej pieniędzy
            case 1:
                gameFailCanvas.SetActive(true);
                FailEndListenersText.SetActive(false);
                FailMoneyText.SetActive(true);
                break;
            //Za mało słuchaczy
            case 2:
                gameFailCanvas.SetActive(true);
                FailEndListenersText.SetActive(true);
                FailMoneyText.SetActive(false);
                break;
        }
    }

    private void ExitGame()
    {
        if (actionManager.GetKeyboardActionType() == ActionTypes.PressedESC)
        {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
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


