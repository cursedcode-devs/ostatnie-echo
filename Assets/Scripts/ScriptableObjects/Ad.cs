using UnityEngine;

[CreateAssetMenu(fileName = "NowaReklama", menuName = "Radio/Reklama")]

/// <summary>
/// Klasa przechowujaca dane reklamy
/// </summary>
public class Ad : PlayableContent
{
    [SerializeField] private string clientName;

    public string GetClientName()
    {
        return clientName;
    }

    public void SetClientName(string client)
    {
        clientName = client;
    }
}
