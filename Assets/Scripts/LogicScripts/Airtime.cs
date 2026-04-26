using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Airtime : MonoBehaviour
{
    [SerializeField] private List<PlayableContent> cassettes = new List<PlayableContent>();

    void Start()
    {
        addSlot(3);
    }

    public GenreValues[] GetGenreValues()
    {
        GenreValues emptyValues = new GenreValues();
        emptyValues.hipHop = 0;
        emptyValues.rock = 0;
        emptyValues.metal = 0;
        emptyValues.disco = 0;
        emptyValues.type = CassetteTypes.Empty;

        List<GenreValues> cassetteValues = new List<GenreValues>();
        foreach (var cassette in cassettes)
        {
            if(cassette==null)
            {
                Debug.Log("Pusta Kaseta");
                continue;
            }
            cassetteValues.Add(cassette.GetCassetteValues());
            Debug.Log("Kaseta dodana");
        }

        return cassetteValues.ToArray();
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
