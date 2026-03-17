using UnityEngine;

[System.Serializable]
public class ObjectSelectionHandler
{
    private float selectTransformValue = 0.3f;
    private float deselectTransformValue = -0.3f;
    [SerializeField] private GameObject selectedObject = null;

    public ObjectSelectionHandler()
    {
        selectedObject = null;
    }

    private void TransformSelectedObject(float transformValue)
    {
        if (selectedObject != null)
            selectedObject.transform.position += new Vector3(0f, transformValue, 0f);
    }

    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }

    public bool IsAnObjectSelected()
    {
        if(selectedObject == null) return false;
        return true;
    }

    public bool IsSelectedObjectPlayable()
    {
        if (!IsAnObjectSelected()) return false;

        if (selectedObject.CompareTag("Playable")) return true;
            
        return false;
    }

    public void DeselectedObject()
    {
        TransformSelectedObject(deselectTransformValue);
        SetDefaultRotation();
        selectedObject = null; ;
    }

    public void SelectObject(GameObject clickedObject)
    {
        if (clickedObject == selectedObject)
        {
            DeselectedObject();
            return;
        }

        if (selectedObject != null)
        {
            DeselectedObject();
        }
        selectedObject = clickedObject;
        TransformSelectedObject(selectTransformValue);
    }


    public void SetDefaultRotation()
    {
        //obiekt wyrówna siê do globalnej osi xyz lub do lokalnej swojego rodzica
        selectedObject.transform.rotation = Quaternion.identity;
    }

    public void RotateObject(float x, float y, float z)
    {
        if (selectedObject == null) return;

        selectedObject.transform.Rotate(x, y, z);
    }
}
