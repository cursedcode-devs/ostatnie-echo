using UnityEngine;

[CreateAssetMenu(fileName = "NoweUlepszenie", menuName = "Radio/Ulepszenie")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("Identyfikacja")]
    public string upgradeName = "Nazwa Ulepszenia";
    [TextArea] public string description = "Opis działania ulepszenia";
    public UpgradeType type;

    [Header("Cena")]
    public float cost = 50f;

    [Header("Back2Back — raz na dzień pojedynczy slot mnoży swój wkład x2")]
    // Typ: Back2Back
    // Brak dodatkowych pól — efekt jest globalny, raz na dzień

    [Header("RollBack — usuwa negatywne efekty z wielokrotnego odtwarzania kasety")]
    // Typ: RollBack
    // Brak dodatkowych pól

    [Header("LuckyDraw — pozwala zrerollować sklep raz (blokuje slot na tę kasetę)")]
    // Typ: LuckyDraw
    // Brak dodatkowych pól

    [Header("MarketFlood — więcej kaset w sklepie")]
    public int marketFloodExtraSlots = 2;

    [Header("NewHorizons — bazowy mnożnik do każdej kategorii")]
    public float newHorizonsBonus = 5f; // dodawane do totalListenerModifier * 0.01 per punkt

    [Header("Gatunek-specyficzne (Disco/Pop/HipHop/Rock)")]
    public float genreBonus = 10f; // dla DiscoNight, PopStars, ComptonVibes, RockAndRoll

    [Header("Waga losowania (wyższe = częstsze)")]
    [Range(1, 100)] public int weight = 10;
}

// ------------------------------------------------------------------

public enum UpgradeType
{
    Back2Back,
    RollBack,
    LuckyDraw,
    MarketFlood,
    NewHorizons,
    DiscoNight,
    PopStars,
    ComptonVibes,
    RockAndRoll
}
