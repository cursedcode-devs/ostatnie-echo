using System;

/// <summary>
/// Statyczna klasa przechowująca dane z kończącego się dnia.
/// Wykorzystywana do przekazania danych z głównej sceny do sceny podsumowania (DaySummaryScene).
/// </summary>
public static class DaySummaryData
{
    public static int Day;
    public static float RentFee;
    public static float FoodFee;
    public static float StudiesFee;
    public static float FinalMoney;
    public static float MoneyDiff;
    
    public static int HipHop;
    public static int HipHopDiff;
    public static int Disco;
    public static int DiscoDiff;
    public static int Rock;
    public static int RockDiff;
    public static int Pop;
    public static int PopDiff;

    // Akcja wywoływana po zamknięciu sceny podsumowania, by wznowić dzień w głównej grze
    public static Action OnSummaryClosed;
}
