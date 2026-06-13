using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CassetteHoverUI : MonoBehaviour
{
    public GameObject hoverUI;
    public Camera mainCamera;
    public new TextMeshProUGUI name;
    public TextMeshProUGUI author;
    public TextMeshProUGUI hipHopStat;
    public TextMeshProUGUI rockStat;
    public TextMeshProUGUI popStat;
    public TextMeshProUGUI discoStat;
    public GameObject hoverImage;
    public RawImage cassetteType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (hoverUI != null)
        {
            if (hipHopStat == null || rockStat == null || popStat == null || discoStat == null)
            {
                Transform rawImage = hoverUI.transform.Find("RawImage");
                if (rawImage != null)
                {
                    if (hipHopStat == null) hipHopStat = rawImage.Find("Statystyki_HipHop")?.GetComponent<TextMeshProUGUI>();
                    if (rockStat == null) rockStat = rawImage.Find("Statystyki_Rock")?.GetComponent<TextMeshProUGUI>();
                    if (popStat == null) popStat = rawImage.Find("Statystyki_Pop")?.GetComponent<TextMeshProUGUI>();
                    if (discoStat == null) discoStat = rawImage.Find("Statystyki_Disco")?.GetComponent<TextMeshProUGUI>();
                }
            }
            hoverUI.SetActive(false);
        }
    }

    public void MouseHover(PlayableContent cassette, PlayableObject playableObject)
    {
        if(cassette==null)
            return;
        if (playableObject == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(playableObject.transform.position);
        screenPosition.x -= 250;
        screenPosition.y += 200;
        hoverImage.transform.position = screenPosition;
        name.text=cassette.GetName();
        if (cassette.GetType() == CassetteTypes.Ad)
        {
            if (author != null) author.gameObject.SetActive(false);
            
            if (hipHopStat != null) hipHopStat.text = FormatStat(cassette.GetHipHop() / 100f, false);
            if (rockStat != null) rockStat.text = FormatStat(cassette.GetRock() / 100f, false);
            if (popStat != null) popStat.text = FormatStat(cassette.GetPop() / 100f, false);
            if (discoStat != null) discoStat.text = FormatStat(cassette.GetDisco() / 100f, false);
        }
        else if (cassette.GetType() == CassetteTypes.Music)
        {
            if (author != null) 
            {
                author.gameObject.SetActive(true);
                author.text=cassette.GetAuthor();
            }
            
            if (hipHopStat != null) hipHopStat.text = FormatStat(cassette.GetHipHop() / 100f, true);
            if (rockStat != null) rockStat.text = FormatStat(cassette.GetRock() / 100f, true);
            if (popStat != null) popStat.text = FormatStat(cassette.GetPop() / 100f, true);
            if (discoStat != null) discoStat.text = FormatStat(cassette.GetDisco() / 100f, true);
        }
        hoverUI.SetActive(true);
    }

    public void MouseLeft()
    {
        hoverUI.SetActive(false);
    }

    private string FormatStat(float value, bool isMusic)
    {
        string colorTag = value > 0 ? "<color=#00FF22>" : (value < 0 ? "<color=#FF0021>" : "");
        string endColorTag = colorTag != "" ? "</color>" : "";
        string prefix = value > 0 ? "+" : "";
        
        if (isMusic)
        {
            return $"{colorTag}{prefix}{value:0.##%}{endColorTag}";
        }
        else
        {
            return $"{colorTag}{prefix}{value}zł{endColorTag}";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
