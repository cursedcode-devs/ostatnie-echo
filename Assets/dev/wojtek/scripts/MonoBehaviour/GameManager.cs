using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public RadioStation radioStation;
    private GameObject selectedCassette;
    [SerializeField] private AudioSource source;
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
    }

    // Update is called once per frame
    void Update()
    {
        actionType = actionManager.actionType();

        switch (actionType)
        {
            case ActionTypes.LeftClickOnObject:
                GameObject clickedObject = actionManager.GetClickedObject();
                if (clickedObject.CompareTag("PlayableCassette") || clickedObject.CompareTag("PlayableAd"))
                {
                    if (clickedObject != selectedCassette)
                    {
                        if (selectedCassette != null) TransformSelectedCassette(deselectTransformValue);
                        selectedCassette = clickedObject;
                        TransformSelectedCassette(selectTransformValue);
                    }
                    else
                    {
                        TransformSelectedCassette(deselectTransformValue);
                        selectedCassette = null;
                    }
                }
                else if (clickedObject.CompareTag("CassettePlayer"))
                {
                    if (!playing && selectedCassette != null)
                    {
                        if (selectedCassette.CompareTag("PlayableCassette"))
                        {
                            selectedCassette.GetComponent<CassetteObject>().data.Play(ref source);
                            selectedCassette.GetComponent<CassetteObject>().data.ApplyEffect(radioStation);
                        }
                        else if (selectedCassette.CompareTag("PlayableAd"))
                        {
                            selectedCassette.GetComponent<AdObject>().data.Play(ref source);
                            selectedCassette.GetComponent<AdObject>().data.ApplyEffect(radioStation);
                        }
                            playing = true;
                    }
                    else
                    {
                        source.Stop();
                        playing = false;
                    }
                }
                break;
            case ActionTypes.LeftClickOutsiedObject:
                if (selectedCassette != null) TransformSelectedCassette(deselectTransformValue);
                selectedCassette = null;
                break;
        }
    }

    private void TransformSelectedCassette(float transformValue)
    {
        selectedCassette.transform.position += new Vector3(0f, transformValue, 0f);
    }

}
