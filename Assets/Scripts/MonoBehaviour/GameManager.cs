using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public RadioStation radioStation;
    private GameObject selectedCassette;
    [SerializeField] private AudioSource source;
    [SerializeField] private Camera mainCamera;
    private bool playing = false;
    private bool miniGameInProgress = false;
    private ActionTypes actionType;
    public ActionManager actionManager;
    public TimeHandler timeHandler;
    [SerializeReference] public MiniGame miniGame;
    public ConsoleSliderObject consoleSliderAmplitude;
    public ConsoleSliderObject consoleSliderFrequency;
    public ConsoleSliderObject consoleSliderLength;


    private float selectTransformValue = 0.3f;
    private float deselectTransformValue = -0.3f;

    private int startHour = 14;
    private int startDay = 1;

    private float startingModifier = 0f;

    void Start()
    {
        selectedCassette = null;

        actionManager = new ActionManager(mainCamera);
        radioStation = new RadioStation();
        timeHandler = new TimeHandler(startHour, startDay);


        radioStation.SetHourlyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetDailyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetHourlyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        radioStation.SetDailyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);
    }

    void Update()
    {
        MiniGameCheckWinCondition();

        actionType = actionManager.GetActionType();
        GameObject clickedObject = actionManager.GetPointedObject();
        ConsoleSliderObject sliderObject = null;

        if (clickedObject != null)
            sliderObject = clickedObject.GetComponent<ConsoleSliderObject>();

        switch (actionType)
        {
            case ActionTypes.LeftClickOnPlayableObject:
                Debug.Log("GetActionType - LeftClickOnPlayableObject");
                SelectPlayableObject(clickedObject);
                break;
            case ActionTypes.LeftClickOnPlayingObject:
                Debug.Log("GetActionType - LeftClickOnPlayingObject");
                PlayPlayableObject(clickedObject);
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
                if (selectedCassette != null) TransformSelectedCassette(deselectTransformValue);
                selectedCassette = null;
                break;
        }
    }

    private void SelectPlayableObject(GameObject clickedObject)
    {
        if (clickedObject == selectedCassette)
        {
            TransformSelectedCassette(deselectTransformValue);
            selectedCassette = null;
            return;
        }

        if (selectedCassette != null)
        {
            TransformSelectedCassette(deselectTransformValue);
        }
        selectedCassette = clickedObject;
        TransformSelectedCassette(selectTransformValue);
    }

    private void PlayPlayableObject(GameObject clickedObject)
    {
        if (miniGameInProgress)
            return;

        if (playing || selectedCassette == null)
        {
            StopPlayingAudio();
            return;
        }

        PlayableObject playableObject = selectedCassette.GetComponent<PlayableObject>();

        if (playableObject != null && playableObject.data != null)
        {
            playableObject.data.Play(ref source);
            playableObject.data.ApplyEffect(radioStation);

            PlayMiniGame();

            playing = true;
            StartCoroutine(WaitForAudioToEnd());
        }
    }

    private void StopPlayingAudio()
    {
        source.Stop();
        playing = false;
    }

    private void MiniGameCheckWinCondition()
    {
        if (!miniGameInProgress) return;

        bool winStatus = miniGame.CheckWinCondition();

        if (winStatus)
        {
            StopPlayingAudio();
            miniGame.AddModifier(radioStation);
        }
            
        miniGameInProgress = !winStatus;
    }

    private void PlayMiniGame()
    {
        if (miniGameInProgress) return;
        miniGameInProgress = true;
        Debug.Log("PlayMiniGame - GameManager");
        miniGame.Play();
    }

    private void TransformSelectedCassette(float transformValue)
    {
        selectedCassette.transform.position += new Vector3(0f, transformValue, 0f);
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
            radioStation.SetHourlyListenersModifier(startingModifier, startingModifier, startingModifier, startingModifier);
            radioStation.SetHourlyRevenueModifier(startingModifier, startingModifier, startingModifier, startingModifier);
        }
    }
}
