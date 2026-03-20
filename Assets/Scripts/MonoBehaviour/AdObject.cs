using UnityEngine;

public class AdObject : PlayableObject
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
