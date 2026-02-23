using Unity.VisualScripting;
using UnityEngine;

//
// Klasa podpinaj¹ca dane do kaset
//
public class CassetteObject : PlayableObject
{
    void Start()
    {
        if (data != null)
        {
            gameObject.tag = "PlayableCassette";
            Debug.Log("To jest fizyczna kopia kasety: " + data.name);
        }
    }
}
