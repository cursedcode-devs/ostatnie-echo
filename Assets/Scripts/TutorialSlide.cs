using UnityEngine;

[System.Serializable]
public class TutorialSlide
{
    [TextArea(5,15)]
    public string text;

    public ZoomTarget cameraTarget;

    public RectTransform slideLayout;
    public GameObject[] elementsToShow;

}