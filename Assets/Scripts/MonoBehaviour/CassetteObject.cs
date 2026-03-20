using Unity.VisualScripting;
using UnityEngine;

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
            data.ResetTimesUsed();
            Debug.Log("To jest fizyczna kopia kasety: " + data.name);
        }
    }
}
