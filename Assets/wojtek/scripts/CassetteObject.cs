using UnityEngine;

public class CassetteObject : MonoBehaviour
{
    [Header("Dane z ScriptableObject")]
    public Cassette data; // Przeci¹gasz tu swój plik .asset

    [Header("Model3D")]
    public MeshRenderer modelRenderer;

    void Start()
    {
        // Przyk³ad: Jeœli w danych kasety masz kolor lub teksturê, 
        // mo¿esz j¹ tu ustawiæ na modelu w momencie startu gry.
        if (data != null)
        {
            Debug.Log("To jest fizyczna kopia kasety: " + data.name);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
