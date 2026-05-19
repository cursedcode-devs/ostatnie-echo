using UnityEngine;

[System.Serializable]
public class ObjectSelectionHandler : MonoBehaviour
{
    [SerializeField] private GameObject selectedObject = null;
    public GameObject selectedObjPos;
    private GameObject lastSelectedObject = null;
    private Vector3 deselectPos = Vector3.zero;
    private Quaternion deselectRot = Quaternion.identity;

    public ObjectSelectionHandler()
    {
        selectedObject = null;
    }

    public Vector3 GetDesPos()
    {
        return deselectPos;
    }

    public Quaternion GetDesRot()
    {
        return deselectRot;
    }

    public void SetSelectedObjPos(GameObject obj)
    {
        selectedObjPos = obj;
    }

    public void SetDeselectPos(Vector3 pos)
    {
        deselectPos = pos;
    }

    public void SetDeselectRot(Quaternion rot)
    {
        deselectRot = rot;
    }

    private void TransformSelectedObject()
    {
        if (selectedObject == null)
            return;
        if (selectedObject.CompareTag("CassettePlayer"))
            return;
        if (selectedObject.CompareTag("Slot"))
            return;
        //selectedObject.transform.position += new Vector3(0f, transformValue, 0f);
        SetDeselectPos(selectedObject.transform.position);
        SetDeselectRot(selectedObject.transform.rotation);
        selectedObject.transform.position = selectedObjPos.transform.position;

        if (selectedObject.CompareTag("Playable"))
            selectedObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    }

    private void TransformDeselectObj()
    {
        if (selectedObject == null)
            return;
        if (selectedObject.CompareTag("CassettePlayer"))
            return;
        if (selectedObject.CompareTag("Slot"))
            return;
        selectedObject.transform.position = deselectPos;
    }

    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }

    public bool IsAnObjectSelected()
    {
        if (selectedObject == null) return false;
        return true;
    }

    public bool IsObjectPlayable()
    {
        if (!IsAnObjectSelected()) return false;

        if (selectedObject.CompareTag("Playable")) return true;

        return false;
    }

    public void DeselectedObject(bool returnToPrevPos = true, bool returnDefaultRot = true)
    {
        if (returnToPrevPos == true)
            TransformDeselectObj();
        if (returnDefaultRot == true)
            SetDefaultRotation();
        lastSelectedObject = selectedObject;
        selectedObject = null;
    }

    public GameObject GetLastSelectedObject()
    {
        return lastSelectedObject;
    }

    public void ResetLastSelectedObject()
    {
        lastSelectedObject = null;
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
        TransformSelectedObject();
    }


    public void SetDefaultRotation()
    {
        if (selectedObject == null)
            return;
        if (selectedObject.CompareTag("CassettePlayer"))
            return;
        if (selectedObject.CompareTag("Slot"))
            return;

        selectedObject.transform.rotation = deselectRot;
        //obiekt wyrówna siê do globalnej osi xyz lub do lokalnej swojego rodzica
        //selectedObject.transform.rotation = Quaternion.identity;
    }

    public void RotateObject(float x, float y, float z)
    {
        if (selectedObject == null)
            return;
        if (selectedObject.CompareTag("CassettePlayer"))
            return;
        if (selectedObject.CompareTag("Slot"))
            return;

        selectedObject.transform.Rotate(x, y, z);
    }
}
