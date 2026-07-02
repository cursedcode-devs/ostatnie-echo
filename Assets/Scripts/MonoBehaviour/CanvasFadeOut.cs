using System.Collections;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(CanvasGroup))]
public class CanvasFadeOut : MonoBehaviour
{
    public float delay = 3f;       // Czas przed rozpoczęciem zanikania
    public float fadeDuration = 1f; // Jak długo trwa fade
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
  
    }

 
    private IEnumerator showDayAndFadeOut(int day)
    {
        canvasGroup.alpha = 1f;
        textMesh.text = string.Format("DZIEŃ: {0}", day);
        yield return new WaitForSeconds(delay);

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;



    }

    public void startShowingDay(int day){
        StartCoroutine(showDayAndFadeOut(day));

    }
}