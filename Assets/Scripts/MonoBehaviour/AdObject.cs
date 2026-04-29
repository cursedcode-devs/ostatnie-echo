using UnityEngine;

public class AdObject : PlayableObject
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
