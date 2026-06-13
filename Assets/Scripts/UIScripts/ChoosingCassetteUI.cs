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
    //public GameObject[] cassetteSlots;
    //public TextMeshProUGUI[] cassetteSlotTexts;
    public TextMeshProUGUI[] StatTexts;
    public TextMeshProUGUI[] equationTexts;
    
    private bool active = false;

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
               // cassetteSlotTexts[i].text = "Slot " + (i + 1);
                StatTexts[i].text = "Hip Hop: \nRock: \nPop: \nDisco Polo: ";
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



            if (c.GetType() == CassetteTypes.Ad)
            {
                StatTexts[i].text = $"Hip Hop: {h / 100f:0.##}zł\nRock: {r / 100f:0.##}zł\nPop: {p / 100f:0.##}zł\nDisco Polo: {d / 100f:0.##}zł";
                sumAdHipHop += h / 100f;
                sumAdRock += r / 100f;
                sumAdPop += p / 100f;
                sumAdDisco += d / 100f;
            }
            else if (c.GetType() == CassetteTypes.Music)
            {
                StatTexts[i].text = $"Hip Hop: {h / 100f:0.##%}\nRock: {r / 100f:0.##%}\nPop: {p / 100f:0.##%}\nDisco Polo: {d / 100f:0.##%}";
                sumMusicHipHop += h / 100f;
                sumMusicRock += r / 100f;
                sumMusicPop += p / 100f;
                sumMusicDisco += d / 100f;
            }

            simulatedTimesUsed[c]++;
        }

        StatTexts[3].text = $"Hip Hop: {sumMusicHipHop:0.##%} {sumAdHipHop:0.##}zł\n" +
                            $"Rock: {sumMusicRock:0.##%} {sumAdRock:0.##}zł\n" +
                            $"Pop: {sumMusicPop:0.##%} {sumAdPop:0.##}zł\n" +
                            $"Disco Polo: {sumMusicDisco:0.##%} {sumAdDisco:0.##}zł";
    }

    private void PlayCassetteSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(gameManager.putCasetteInSound, cassettePlayer.transform.position);
    }

    public void Show()
    {
        choosingCassetteCanvas.SetActive(true);
        UpdatePredictions();
    }

    public void Hide()
    {
        //choosingCassetteCanvas.SetActive(false);
    }

    //public void ResetSlotText()
    //{
    //    for (int i = 0; i < cassetteSlotTexts.Length; i++)
    //    {
    //        cassetteSlotTexts[i].text = "Slot " + (i + 1);
    //    }
    //}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
