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
    public TextMeshProUGUI stats;
    public GameObject hoverImage;
    public RawImage cassetteType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(hoverUI != null)
            hoverUI.SetActive(false);
    }

    public void MouseHover(PlayableContent cassette, PlayableObject playableObject)
    {
        if(cassette==null)
            return;
        if (playableObject == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(playableObject.transform.position);
        screenPosition.x += 140;
        screenPosition.y -= 90;
        hoverImage.transform.position = screenPosition;
        name.text=cassette.GetName();
        author.text=cassette.GetAuthor();
        if (cassette.GetType() == CassetteTypes.Ad)
        {
            stats.text = "HipHop: " + cassette.GetHipHop() / 100f + "z�" + "\nRock: " + cassette.GetRock() / 100f + "z�" + "\nPop: " + cassette.GetPop() / 100f + "z�" + "\nDisco: " + cassette.GetDisco() / 100f + "z�";
        }
        else if (cassette.GetType() == CassetteTypes.Music)
        {
            stats.text = $"HipHop: {cassette.GetHipHop() / 100f:0.##%}\nRock: {cassette.GetRock() / 100f:0.##%}\nPop: {cassette.GetPop() / 100f:0.##%}\nDisco: {cassette.GetDisco() / 100f:0.##%}";
        }
        hoverUI.SetActive(true);
    }

    public void MouseLeft()
    {
        hoverUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
