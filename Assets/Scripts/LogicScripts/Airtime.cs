using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Airtime : MonoBehaviour
{
    [SerializeField] private List<PlayableContent> cassettes = new List<PlayableContent>();
    [SerializeField] private PlayableContent emptyCassette;

    void Start()
    {
        addSlot(3);
    }

    public PlayableContent[] GetCassettes()
    {
        for (int i = 0; i < cassettes.Count; i++)
        {
            if(cassettes[i] ==  null)
            {
                cassettes[i] = emptyCassette;
            }
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
    }

    public void emptySlot(int slot)
    {
        cassettes[slot] = null;
    }

    public void emptyAllSlots()
    {
        for (int i = 0; i < cassettes.Count; i++)
        {
            cassettes[i] = null;
        }
    }

}
