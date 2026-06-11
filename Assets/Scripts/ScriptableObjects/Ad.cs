using UnityEngine;

[CreateAssetMenu(fileName = "NowaReklama", menuName = "Radio/Reklama")]

/// <summary>
/// Klasa przechowujaca dane reklamy
/// </summary>
public class Ad : PlayableContent
{
    [SerializeField] private string clientName;

    [TextArea(4, 15)]
    [Tooltip("Treść reklamy odczytywana na antenie. Wyświetlana jako napisy (dla osób niesłyszących / dla jasności), zsynchronizowane z dźwiękiem odczytu (pole 'audio').")]
    [SerializeField] private string content;

    public string GetClientName()
    {
        return clientName;
    }

    public void SetClientName(string client)
    {
        clientName = client;
    }

    /// <summary>
    /// Zwraca pełną treść reklamy (tekst odczytu). Może być pusta.
    /// </summary>
    public string GetContent()
    {
        return content;
    }

    public void SetContent(string newContent)
    {
        content = newContent;
    }
}
