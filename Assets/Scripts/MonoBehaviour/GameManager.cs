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

    private MiniGame[] availableMiniGames = new MiniGame[2];    // index 0 - no minigame

    public ConsoleSliderObject amplitudeSlider;
    public ConsoleSliderObject lengthSlider;
    public ConsoleSliderObject frequencySlider;
    public GameObject WaveTweakingUI;

    private float selectTransformValue = 0.3f;
    private float deselectTransformValue = -0.3f;

    private int startHour = 14;
    private int startDay = 1;

    private float startingModifier = 0f;

    private bool miniGameWinStatus = false;

    void Start()
    {
        availableMiniGames[0] = null;
        availableMiniGames[1] = new WaveTweakingMiniGame(amplitudeSlider, lengthSlider, frequencySlider, WaveTweakingUI);
        miniGame = availableMiniGames[1];
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

    private void SelectRandomMiniGame()
    {
        int drawnMiniGame = Random.Range(0, availableMiniGames.Length - 1);

        switch (drawnMiniGame)
        {
            case 0:
                miniGame = null;
                break;
            case 1:
                miniGame = availableMiniGames[1];
                break;
        }

    }

    private void MiniGameCheckWinCondition()
    {
        if (!miniGameInProgress) return;

        miniGameWinStatus = miniGame.CheckWinCondition();

        if (miniGameWinStatus)
        {
            StopPlayingAudio();
        }

        miniGameInProgress = !miniGameWinStatus;
    }

    private void PlayMiniGame()
    {
        if (miniGameInProgress) return;
        SelectRandomMiniGame();
        miniGameInProgress = true;
        Debug.Log("PlayMiniGame - GameManager");
        if (miniGame != null)
            miniGame.Start();
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
            timeHandler.NextHour(radioStation, startingModifier);
        }

        if (miniGameWinStatus)
        {
            miniGame.AddModifier(radioStation);
        }

    }
}
