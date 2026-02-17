using UnityEngine;

public class AdObject : MonoBehaviour
{
    [Header("Dane z ScriptableObject")]
    public Ad data;

    void Start()
    {
        if (data != null)
        {
            gameObject.tag = "PlayableAd";
            Debug.Log("To jest fizyczna kopia kasety: " + data.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
