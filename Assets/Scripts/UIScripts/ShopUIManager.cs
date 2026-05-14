using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menedżer UI sklepu wewnątrz ekranu podsumowania.
/// Łączy przyciski z instancją DayEndHandler.
/// </summary>
public class ShopUIManager : MonoBehaviour
{
    [Header("Przyciski")]
    public Button buyBtn1;
    public Button buyBtn2;
    public Button buyBtn3;
    public Button exitBtn;

    void Start()
    {
        var handler = DayEndHandler.Instance;
        if (handler != null)
        {
            if (buyBtn1 != null) { buyBtn1.onClick.RemoveAllListeners(); buyBtn1.onClick.AddListener(() => handler.BuyCassette(0)); }
            if (buyBtn2 != null) { buyBtn2.onClick.RemoveAllListeners(); buyBtn2.onClick.AddListener(() => handler.BuyCassette(1)); }
            if (buyBtn3 != null) { buyBtn3.onClick.RemoveAllListeners(); buyBtn3.onClick.AddListener(() => handler.BuyCassette(2)); }
        }

        var summaryMgr = FindAnyObjectByType<DaySummarySceneManager>();
        if (summaryMgr != null && exitBtn != null)
        {
            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(() => summaryMgr.OnShopContinueClicked());
        }
    }
}