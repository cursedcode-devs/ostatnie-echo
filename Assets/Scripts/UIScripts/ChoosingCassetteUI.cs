using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.Rendering.DebugUI;

public class ChoosingCassetteUI : MonoBehaviour
{
    public GameObject choosingCassetteCanvas;
    public GameManager gameManager;
    public GameObject cassettePlayer;
    public Airtime airtime;
    private GameObject selectedObject;
    private PlayableObject playableObject;
    //public GameObject[] cassetteSlots;
    public TextMeshProUGUI[] cassetteSlotTexts;
    public TextMeshProUGUI[] StatTexts;
    public TextMeshProUGUI[] equationTexts;
    private bool active = false;
    public void clickedSlot(int slot)
    {
        // LastSelectedObject bo jak klikniesz lewym na przycisk slotu to kaseta zd¹¿y siê odznaczyæ
        selectedObject = gameManager.selectionHandler.GetLastSelectedObject();

        if (selectedObject == null || !selectedObject.CompareTag("Playable"))
        {
            airtime.emptySlot(slot);
            PlayCassetteSound();
        }
        else if (selectedObject.CompareTag("Playable"))
        {
            playableObject = selectedObject.GetComponent<PlayableObject>();
            airtime.setSlot(playableObject.data, slot);
            gameManager.selectionHandler.ResetLastSelectedObject();
            PlayCassetteSound();
        }

        // Zamiast starego kodu, wywo³ujemy now¹ funkcjê symuluj¹c¹, która obliczy przewidywania dla wszystkich slotów
        UpdatePredictions();
    }

    public void UpdatePredictions()
    {
        PlayableContent[] cassettes = airtime.GetCassettes();

        Dictionary<PlayableContent, int> simulatedTimesUsed = new Dictionary<PlayableContent, int>();
        Dictionary<PlayableContent, GenreValues> simulatedLastValues = new Dictionary<PlayableContent, GenreValues>();

        float sumMusicHipHop = 0f, sumMusicRock = 0f, sumMusicPop = 0f, sumMusicDisco = 0f;
        float sumAdHipHop = 0f, sumAdRock = 0f, sumAdPop = 0f, sumAdDisco = 0f;

        

        for (int i = 0; i < cassettes.Length; i++)
        {
            if (cassettes[i] == null)
            {
                cassetteSlotTexts[i].text = "Slot " + (i + 1);
                StatTexts[i].text = "HipHop: \nRock: \nPop: \nDisco: ";
                continue;
            }

            PlayableContent c = cassettes[i];

            if (!simulatedTimesUsed.ContainsKey(c))
            {
                simulatedTimesUsed[c] = c.GetTimesUsed();

                GenreValues initialVals = c.GetTimesUsed() > 0 ? c.GetLastValues() : c.GetCassetteValues();

                simulatedLastValues[c] = new GenreValues
                {
                    hipHop = initialVals.hipHop,
                    rock = initialVals.rock,
                    pop = initialVals.pop,
                    disco = initialVals.disco
                };
            }

            int currentUses = simulatedTimesUsed[c];
            GenreValues currentVals = simulatedLastValues[c];

            int h = currentVals.hipHop;
            int r = currentVals.rock;
            int p = currentVals.pop;
            int d = currentVals.disco;

            if (currentUses > 0 && c.GetType() == CassetteTypes.Music)
            {
                h = h - Mathf.Abs(h / 2);
                r = r - Mathf.Abs(r / 2);
                p = p - Mathf.Abs(p / 2);
                d = d - Mathf.Abs(d / 2);

                simulatedLastValues[c] = new GenreValues { hipHop = h, rock = r, pop = p, disco = d };
            }

            // Aktualizacja UI dla konkretnego slotu
            cassetteSlotTexts[i].text = c.GetName() + " U¿ycie: " + currentUses;

            if (c.GetType() == CassetteTypes.Ad)
            {
                StatTexts[i].text = $"HipHop: {h / 100f:0.##}z³\nRock: {r / 100f:0.##}z³\nPop: {p / 100f:0.##}z³\nDisco: {d / 100f:0.##}z³";
                sumAdHipHop += h / 100f;
                sumAdRock += r / 100f;
                sumAdPop += p / 100f;
                sumAdDisco += d / 100f;
            }
            else if (c.GetType() == CassetteTypes.Music)
            {
                StatTexts[i].text = $"HipHop: {h / 100f:0.##}\nRock: {r / 100f:0.##}\nPop: {p / 100f:0.##}\nDisco: {d / 100f:0.##}";
                sumMusicHipHop += h / 100f;
                sumMusicRock += r / 100f;
                sumMusicPop += p / 100f;
                sumMusicDisco += d / 100f;
            }

            simulatedTimesUsed[c]++;
        }

        // Aktualizacja sumy ca³kowitej w ostatnim polu (zak³adam, ¿e to StatTexts[3])
        StatTexts[3].text = $"HipHop: {sumMusicHipHop:0.##} {sumAdHipHop:0.##}z³\n" +
                            $"Rock: {sumMusicRock:0.##} {sumAdRock:0.##}z³\n" +
                            $"Pop: {sumMusicPop:0.##} {sumAdPop:0.##}z³\n" +
                            $"Disco: {sumMusicDisco:0.##} {sumAdDisco:0.##}z³";
    }

    private void PlayCassetteSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(gameManager.putCasetteInSound, cassettePlayer.transform.position);
    }

    public void ToggleVisibility(bool isPlaying)
    {
        if (isPlaying)
            return;
        choosingCassetteCanvas.SetActive(!active);
        active = !active;
    }

    public void Show()
    {
        UpdatePredictions();
        choosingCassetteCanvas.SetActive(true);
    }

    public void Hide()
    {
        choosingCassetteCanvas.SetActive(false);
    }

    public void ResetSlotText()
    {
        for (int i = 0; i < cassetteSlotTexts.Length; i++)
        {
            cassetteSlotTexts[i].text = "Slot " + (i + 1);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
