using UnityEngine;
using TMPro;

public class InstructionScript : MonoBehaviour
{
    public TextMeshProUGUI output;

    public void ButtonClicked()
    {
        Destroy(output);
    }
}
