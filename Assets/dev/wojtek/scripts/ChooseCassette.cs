using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

public class ChooseCassette : MonoBehaviour
{
    [SerializeField]
    private Camera camera;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //casetteChoose();
        //playMusic();
    }

    public GameObject SelectingObject(Vector3 mousePos)
    {     
        Ray clickingRay = camera.ScreenPointToRay(mousePos);
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




    //void casetteChoose()
    //{
    //    mousePosition = Mouse.current.position.ReadValue();

    //    Ray clickingRay = camera.ScreenPointToRay(mousePosition);
    //    RaycastHit raycastHit;
    //    bool objectHited = Physics.Raycast(clickingRay, out raycastHit);

    //    if (objectHited)
    //    {
    //        Debug.Log(raycastHit.transform.name);
    //    }


    //    bool leftMousePressed = Mouse.current.leftButton.wasPressedThisFrame;

    //    if (objectHited && leftMousePressed && raycastHit.transform.gameObject != selectedObject)
    //    {
    //        raycastHit.transform.position += new Vector3(0, 0.3f, 0);
    //        if (selectedObject != null)
    //        {
    //            selectedObject.transform.position += new Vector3(0, -0.3f, 0);
    //            selectedObject = null;
    //        }
    //        selectedObject = raycastHit.transform.gameObject;

    //        switch (selectedObject.transform.name)
    //        {
    //            case "kaseta HipHop":
    //                source.clip = hiphopClip;
    //                break;
    //            case "kaseta Disco":
    //                source.clip = discoClip;
    //                break;
    //            case "kaseta Rock":
    //                source.clip = rockClip;
    //                break;
    //            case "kaseta Metal":
    //                source.clip = metalClip;
    //                break;
    //        }
    //    }
    //    else if (objectHited && leftMousePressed && raycastHit.transform.gameObject == selectedObject)
    //    {
    //        selectedObject.transform.position += new Vector3(0, -0.3f, 0);
    //        selectedObject = null;
    //    }
    //    else if (!objectHited && leftMousePressed && selectedObject != null)
    //    {
    //        selectedObject.transform.position += new Vector3(0, -0.3f, 0);
    //        selectedObject = null;
    //    }
    //}


    //void playMusic()
    //{
    //    // Sprawdzamy czy spacja wciœniêta ORAZ czy mamy przypisane Ÿród³o dŸwiêku
    //    if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
    //    {
    //        if (source != null && playing == false)
    //        {
    //            Debug.Log("SPACJA! Gram dŸwiêk.");
    //            source.Play();
    //            playing = true;
    //        }
    //        else if (source != null && playing == true)
    //        {
    //            source.Stop();
    //            playing = false;
    //        }
    //        else
    //        {
    //            Debug.LogError("Zapomnia³eœ przypisaæ AudioSource do skryptu w inspektorze!");
    //        }
    //    }
    //}

}
