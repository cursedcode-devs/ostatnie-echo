using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Airtime : MonoBehaviour
{
   [SerializeField] private List<PlayableContent> cassettes = new List<PlayableContent>();
    public CassetteSlotHandler[] cassetteSlots;
   public ChoosingCassetteUI cassetteUI;
    void Start()
    {
        addSlot(3);
    }

    public bool AreSlotsClosed()
    {

        for (int i = 0; i < cassetteSlots.Length; i++)
        {
            if (!cassetteSlots[i].isSlotOpen())
                return false;
        }
        
        return true;
    }

    public float GetStatsSum(int genre, CassetteTypes type = CassetteTypes.Music)
    {
        float total = 0f;

        for (int i = 0; i < cassettes.Count; i++)
        {
            if (cassettes[i] == null)
                continue;
            if (cassettes[i].GetType() != type)
                continue;

                switch (genre)
                {
                    case 0:
                        total += cassettes[i].GetHipHop();
                        break;
                    case 1:
                        total += cassettes[i].GetRock();
                        break;
                    case 2:
                        total += cassettes[i].GetPop();
                        break;
                    case 3:
                        total += cassettes[i].GetDisco();
                        break;
                }
        }

        return total/100f;
    }

    public PlayableContent[] GetCassettes()
    {
        for (int i = 0; i < cassettes.Count; i++)
        {
            //if (cassettes[i] == null)
            //{
            //    //cassettes[i] = emptyCassette;
            //}
        }

        return cassettes.ToArray();
    }

    public AudioClip[] GetCassettesAudio()
    {
        List<AudioClip> audios = new List<AudioClip>();

        for (int i = 0; i < cassettes.Count; i++)
        {
            if (cassettes[i] == null)
            {
                audios.Add(null);
            }
            else
            {
                audios.Add(cassettes[i].audio);
            }
        }

        return audios.ToArray();
    }

    public void addSlot(int slotsAmount)
    {
        for (int i = 0; i < slotsAmount; i++)
        {
            cassettes.Add(null);
        }
    }

    public void removeLastSlot()
    {
        if (cassettes.Count == 0)
        {
            Debug.Log("Nie moge usunac slotu, bo juz nie ma ¿adnych slotów");
            return;
        }
        cassettes.RemoveAt(cassettes.Count - 1);
    }

    public void setSlot(PlayableContent cassette, int slot)
    {
        if (slot < 0)
        {
            Debug.Log("Wybrano slot mniejszy od 0");
            return;
        }

        if (slot > cassettes.Count - 1)
        {
            Debug.Log("Wybrano slot mniejszy od 0");
            return;
        }

        cassettes[slot] = cassette;

        cassetteUI.UpdatePredictions();
    }

    public void emptySlot(int slot)
    {
        cassettes[slot] = null;
        cassetteUI.UpdatePredictions();
    }

    public void emptyAllSlots()
    {
        for (int i = 0; i < cassettes.Count; i++)
        {
            cassettes[i] = null;
        }
        cassetteUI.UpdatePredictions();
    }

}
