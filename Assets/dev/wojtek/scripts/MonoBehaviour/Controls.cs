using UnityEngine;


/// <summary>
/// Klasa do obs³ugi sterowania
/// </summary>
public class Controls : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject SelectingObject(Vector3 mousePos)
    {
        Ray clickingRay = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit raycastHit;
        bool objectHited = Physics.Raycast(clickingRay, out raycastHit);

        if (objectHited)
        {
            Debug.Log(raycastHit.transform.name);
        }

        if (objectHited) //klikniêto w obiekt z colliderem
        {
            return raycastHit.transform.gameObject;
        }
        else if (!objectHited)  //Nie klikniêto w obiekt z colliderem
        {
            return null;
        }
        return null;
    }
}
