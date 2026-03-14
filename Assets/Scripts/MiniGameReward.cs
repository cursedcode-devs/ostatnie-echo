using UnityEngine;

/// <summary>
/// Jedna nagroda za wygranie minigry.
/// Twórz przez: prawym na folder → Create → MiniGame/Reward
///
/// Przykłady:
///   "Boost HipHop x1.5 na 3 dni"
///   "Gotówka +50$"
///   "Dodaj kasetę do oferty"
/// </summary>
[CreateAssetMenu(fileName = "NowaOffer", menuName = "MiniGame/Reward")]
public class MiniGameReward : ScriptableObject
{
    [Header("Opis (wyświetlany graczowi)")]
    public string rewardName = "Nagroda";
    [TextArea] public string description = "Opis nagrody...";

    [Header("Typ nagrody")]
    public RewardType type;

    [Header("Boosty słuchaczy (RewardType.ListenersBoost)")]
    public GenreValues listenersBoostFlat;        // stały przyrost słuchaczy
    public GenreValuesModifier listenersModifier; // mnożnik (1.5 = +50%)
    public int modifierDurationDays = 1;          // ile dni działa mnożnik (0 = permanent)

    [Header("Boost przychodów (RewardType.RevenueBoost)")]
    public GenreValuesModifier revenueModifier;
    public int revenueModifierDurationDays = 1;

    [Header("Gotówka (RewardType.Money)")]
    public float moneyAmount = 50f;

    [Header("Kaseta (RewardType.Cassette)")]
    public Cassette cassetteToAdd;   // kaseta dodawana do dailyOffer

    [Header("Waga losowania (wyżej = częściej)")]
    [Range(1, 100)] public int weight = 10;

    // ------------------------------------------------------------------
    /// <summary>Zastosuj nagrodę na stację radiową.</summary>
    public void Apply(RadioStation radio, DayEndHandler dayEndHandler = null)
    {
        switch (type)
        {
            case RewardType.ListenersBoost:
                // Stały przyrost
                if (listenersBoostFlat.totalListeners > 0)
                    radio.AddListeners(ToPercentage(listenersBoostFlat, radio));

                // Mnożnik (tymczasowy lub stały)
                if (HasNonDefaultModifier(listenersModifier))
                {
                    radio.setListenersModifier(
                        listenersModifier.hipHop,
                        listenersModifier.disco,
                        listenersModifier.rock,
                        listenersModifier.metal
                    );
                    Debug.Log($"[Reward] Listeners modifier aktywny przez {modifierDurationDays} dni.");
                }
                break;

            case RewardType.RevenueBoost:
                if (HasNonDefaultModifier(revenueModifier))
                {
                    radio.setRevenueModifier(
                        revenueModifier.hipHop,
                        revenueModifier.disco,
                        revenueModifier.rock,
                        revenueModifier.metal
                    );
                    Debug.Log($"[Reward] Revenue modifier aktywny przez {revenueModifierDurationDays} dni.");
                }
                break;

            case RewardType.Money:
                radio.AddMoney(moneyAmount);
                Debug.Log($"[Reward] +{moneyAmount}$ do kasy.");
                break;

            case RewardType.Cassette:
                if (cassetteToAdd != null && dayEndHandler != null)
                {
                    dayEndHandler.AddCassetteToOffer(cassetteToAdd);
                    Debug.Log($"[Reward] Kaseta '{cassetteToAdd.name}' dodana do oferty.");
                }
                break;
        }

        Debug.Log($"[Reward] Zastosowano: {rewardName}");
    }

    // ------------------------------------------------------------------
    // Helpers

    // Zamienia stały przyrost na procent (żeby użyć istniejącej metody AddListeners)
    private GenreValues ToPercentage(GenreValues flat, RadioStation radio)
    {
        return new GenreValues
        {
            hipHop = radio.currentListeners.hipHop > 0
                ? Mathf.RoundToInt((float)flat.hipHop / radio.currentListeners.hipHop * 100f) : 0,
            disco  = radio.currentListeners.disco > 0
                ? Mathf.RoundToInt((float)flat.disco  / radio.currentListeners.disco  * 100f) : 0,
            rock   = radio.currentListeners.rock > 0
                ? Mathf.RoundToInt((float)flat.rock   / radio.currentListeners.rock   * 100f) : 0,
            metal  = radio.currentListeners.metal > 0
                ? Mathf.RoundToInt((float)flat.metal  / radio.currentListeners.metal  * 100f) : 0,
        };
    }

    private bool HasNonDefaultModifier(GenreValuesModifier m)
        => m.hipHop != 0 || m.disco != 0 || m.rock != 0 || m.metal != 0;
}

public enum RewardType
{
    ListenersBoost,   // boostuje słuchaczy
    RevenueBoost,     // boostuje przychody z reklam
    Money,            // gotówka od razu
    Cassette          // dodaje kasetę do dzisiejszej oferty
}
