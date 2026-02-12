using UnityEngine;

[CreateAssetMenu(fileName = "NowaReklama", menuName = "Radio/Reklama")]
public class Ad : PlayableContent
{
    [SerializeField]
    private string name;
    [SerializeField]
    private int timesUsedInDay;
    GenreValues revenuePerListener; //Ile groszy za jednego s³uchacza 100 -> 1 zl 50 -> 0.50z³.


    public override void ApplyEffect(Radio radio)
    {
        radio.AddRevenue(revenuePerListener);
    }
}
