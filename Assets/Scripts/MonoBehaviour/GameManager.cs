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
    ActionTypes actionType;
    public ActionManager actionManager;
    public TimeHandler timeHandler;
    private StatsUI statsUI; 


    private float selectTransformValue = 0.3f;
    private float deselectTransformValue = -0.3f;

    private int startHour = 14;
    private int startDay = 1;

    private bool inputEnabled = true;


    void Start()
    {
        selectedCassette = null;

        actionManager = new ActionManager(mainCamera);
        radioStation = new RadioStation();
        timeHandler = new TimeHandler(startHour, startDay);

        radioStation.setListenersModifier(1f, 1f, 1f, 1f);
        radioStation.setRevenueModifier(1f, 1f, 1f, 1f);

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
        if (!inputEnabled) return;

        actionType = actionManager.GetActionType();

        switch (actionType)
        {
            case ActionTypes.LeftClickOnObject:
                Debug.Log("GetActionType - LeftClickOnObject");
                GameObject clickedObject = actionManager.GetClickedObject();
                SelectPlayableObject(clickedObject);
                PlayPlayableObject(clickedObject);
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
        if (!clickedObject.CompareTag("PlayableCassette") && !clickedObject.CompareTag("PlayableAd")) return;

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
        if (!clickedObject.CompareTag("CassettePlayer")) return;

        if (playing || selectedCassette == null)
        {
            source.Stop();
            playing = false;
            return;
        }

        PlayableObject playableObject = selectedCassette.GetComponent<PlayableObject>();

        if (playableObject != null && playableObject.data != null)
        {
            playableObject.data.Play(ref source);
            playableObject.data.ApplyEffect(radioStation);
            if (statsUI != null) statsUI.UpdateUI(); 

            playing = true;
            StartCoroutine(WaitForAudioToEnd());
        }
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
            MiniGameSystem.Instance.Launch("LightsOut");
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
}


