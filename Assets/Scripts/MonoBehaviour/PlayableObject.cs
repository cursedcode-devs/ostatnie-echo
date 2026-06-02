using UnityEngine;
using UnityEngine.EventSystems;

public class PlayableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject hoverUIScriptObject;
    public CassetteHoverUI hoverUIScript;

    [Header("Dane z ScriptableObject")]
    public PlayableContent data;

    protected void Start()
    {
        if (data != null)
        {
            gameObject.tag = "Playable";
            Debug.Log("To jest fizyczna kopia kasety: " + data.name);
            data.ResetTimesUsed();
            data.ResetLastValues();
        }

        hoverUIScriptObject = GameObject.Find("CassetteHoverScript");
        if (hoverUIScriptObject != null)
        {
            if (hoverUIScriptObject.GetComponent<CassetteHoverUI>() != null)
            {
                Debug.Log("Pobra³em hoverUIScriptObject prawid³owo");
            }
            hoverUIScript = hoverUIScriptObject.GetComponent<CassetteHoverUI>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverUIScript.MouseHover(data, this);
        Debug.Log("Jestem w kasecie");
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        hoverUIScript.MouseLeft();
        Debug.Log("Wyszed³em z kasety");
    }
}
