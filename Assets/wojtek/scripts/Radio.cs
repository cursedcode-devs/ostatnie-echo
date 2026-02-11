using UnityEngine;
using UnityEngine.InputSystem;

public class Radio : MonoBehaviour
{
    [SerializeField]
    private Camera camera;

    [SerializeField]
    private int listenersTotal = 0;
    [SerializeField]
    private int listenersHipHop = 0;
    [SerializeField]
    private int listenersDisco = 0;
    [SerializeField]
    private int listenersRock = 0;
    [SerializeField]
    private int listenersMetal = 0;

    [SerializeField]
    AudioClip hiphopClip;
    [SerializeField]
    AudioClip discoClip;
    [SerializeField]
    AudioClip rockClip;
    [SerializeField]
    AudioClip metalClip;


    [SerializeField]
    private AudioSource source;

    Vector3 mousePosition;


    GameObject selectedObject=null;

    private bool playing = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        mousePosition= Mouse.current.position.ReadValue();

        Ray clickingRay = camera.ScreenPointToRay(mousePosition);
        RaycastHit raycastHit;
        bool objectHited = Physics.Raycast(clickingRay, out raycastHit);

        if (objectHited)
        {
            Debug.Log(raycastHit.transform.name);
        }


        bool leftMousePressed = Mouse.current.leftButton.wasPressedThisFrame;

        if (objectHited && leftMousePressed && raycastHit.transform.gameObject!=selectedObject)
        {
            raycastHit.transform.position += new Vector3(0, 0.3f, 0);
            if (selectedObject != null)
            {
                selectedObject.transform.position += new Vector3(0, -0.3f, 0);
                selectedObject = null;
            }
            selectedObject = raycastHit.transform.gameObject;

            switch (selectedObject.transform.name)
            {
                case "kaseta HipHop":
                    source.clip = hiphopClip;
                    break;
                case "kaseta Disco":
                    source.clip = discoClip;
                    break;
                case "kaseta Rock":
                    source.clip = rockClip;
                    break;
                case "kaseta Metal":
                    source.clip = metalClip;
                    break;
            }
        }
        else if(objectHited && leftMousePressed && raycastHit.transform.gameObject == selectedObject)
        {
            selectedObject.transform.position += new Vector3(0, -0.3f, 0);
            selectedObject = null;
        }
        else if(!objectHited && leftMousePressed && selectedObject != null)
        {
            selectedObject.transform.position += new Vector3(0, -0.3f, 0);
            selectedObject = null;
        }

        playMusic();
        
    }


    void playMusic()
    {
        // Sprawdzamy czy spacja wciœniêta ORAZ czy mamy przypisane Ÿród³o dŸwiêku
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (source != null && playing == false)
            {
                Debug.Log("SPACJA! Gram dŸwiêk.");
                source.Play();
                playing = true;
            }
            else if (source != null && playing == true)
            {
                source.Stop();
                playing = false;
            }
            else
            {
                Debug.LogError("Zapomnia³eœ przypisaæ AudioSource do skryptu w inspektorze!");
            }
        }
    }
}
