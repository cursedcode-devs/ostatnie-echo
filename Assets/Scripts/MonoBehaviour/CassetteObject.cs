using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
//
// Klasa podpinaj�ca dane do kaset
//
public class CassetteObject : PlayableObject
{
    void Start()
    {
        if (data != null)
        {
            gameObject.tag = "Playable";
            Debug.Log("To jest fizyczna kopia kasety: " + data.name);
        }
    }

}
