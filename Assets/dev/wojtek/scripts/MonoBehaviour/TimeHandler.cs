using UnityEngine;

public class TimeHandler : MonoBehaviour
{
    public int Hour;
    public int Day;
    void Start()
    {
        Day=1;
        Hour=14;
    }
    void Update()
    {
        
    }
    public void NextHour()
    {
        Hour++;
        if (Hour>20)
        {
            NextDay();
        }
    }
    public void NextDay()
    {
        Day++;
        Hour = 14;
        if(Day>7)
        {
            FinishGame();    
        }
    }
    public void FinishGame()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
