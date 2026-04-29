using UnityEngine;
using TMPro;

public class InstructionScript : MonoBehaviour
{
    public GameObject instructionCanvas;
    private int page = 0;

    public GameObject[] instructionPages;
    public GameObject activePage;
    public GameObject prevButton;
    public TextMeshProUGUI nextPageButtonText;
    public GameManager gameManager;

    private void Start()
    {
        activePage = instructionPages[0];
        activePage.SetActive(true);
        prevButton.SetActive(false);
    }

    public void NextButtonClicked()
    {
        page++;

        if(prevButton.activeInHierarchy == false)
        {
            prevButton.SetActive(true);
        }

        if (page == instructionPages.Length - 1)
        {
            nextPageButtonText.text = "Graj!";
        }

        if (page < instructionPages.Length)
        {
            activePage.SetActive(false);
            activePage = instructionPages[page];
            activePage.SetActive(true);
        }
        else
        {
            gameManager.SetInputEnabled(true);
            instructionCanvas.SetActive(false);
            MiniGameSystem.Instance.LaunchRandom();
        }
    }

    public void PrevButtonClicked()
    {
        if(page == instructionPages.Length - 1)
        {
            nextPageButtonText.text = "Nastêpna strona";
        }

        page--;
        if(page == 0)
        {
            prevButton.SetActive(false);
        }

        activePage.SetActive(false);
        activePage = instructionPages[page];
        activePage.SetActive(true);
    }
}