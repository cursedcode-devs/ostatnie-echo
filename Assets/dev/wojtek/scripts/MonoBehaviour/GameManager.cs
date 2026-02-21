using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public RadioStation radioStation;
    private GameObject selectedCassette;
    [SerializeField] private AudioSource source;
    [SerializeField] private TimeHandler timeHandler;
    private bool playing = false;
    ActionTypes actionType;
    public ActionManager actionManager;


    private float selectTransformValue = 0.3f;
    private float deselectTransformValue = -0.3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedCassette = null;

        actionManager = GetComponent<ActionManager>();
        if (actionManager == null) actionManager = gameObject.AddComponent<ActionManager>();

        radioStation = GetComponent<RadioStation>();
        if (radioStation == null) radioStation = gameObject.AddComponent<RadioStation>();

        radioStation.setListenersModifier(1f, 1f, 1f, 1f);
        radioStation.setRevenueModifier(1f, 1f, 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        actionType = actionManager.actionType();

        switch (actionType)
        {
            case ActionTypes.LeftClickOnObject:
                GameObject clickedObject = actionManager.GetClickedObject();
                SelectPlayableObject(clickedObject);
                PlayPlayableObject(clickedObject);
                break;
            case ActionTypes.LeftClickOutsiedObject:
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

        PlayableObject playableContent = selectedCassette.GetComponent<PlayableObject>();

        if (playableContent != null && playableContent.data != null)
        {
            playableContent.data.Play(ref source);
            playableContent.data.ApplyEffect(radioStation);

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
        }
    }
}
