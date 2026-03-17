using UnityEngine;

/// <summary>
/// One reward granted when a minigame is won.
/// Uses RadioStation's layered modifier system — rewards stack
/// rather than replacing each other.
///
/// Create via: right-click → Create → MiniGame/Reward
/// </summary>
[CreateAssetMenu(fileName = "NewReward", menuName = "MiniGame/Reward")]
public class MiniGameReward : ScriptableObject
{
    [Header("Description (shown to player)")]
    public string rewardName = "Reward";
    [TextArea] public string description = "Reward description...";

    [Header("Reward type")]
    public RewardType type;

    [Header("Listener boost (RewardType.ListenersBoost)")]
    public GenreValues listenersBoostFlat;
    public GenreValuesModifier listenersModifier;

    [Tooltip("Hourly = lasts until next hour tick. Daily = lasts until end of day.")]
    public ModifierDuration listenerModifierDuration = ModifierDuration.Daily;

    [Header("Revenue boost (RewardType.RevenueBoost)")]
    public GenreValuesModifier revenueModifier;
    public ModifierDuration revenueModifierDuration = ModifierDuration.Daily;

    [Header("Money (RewardType.Money)")]
    public float moneyAmount = 50f;

    [Header("Cassette (RewardType.Cassette)")]
    public Cassette cassetteToAdd;

    [Header("Draw weight (higher = more common)")]
    [Range(1, 100)] public int weight = 10;

    // ------------------------------------------------------------------
    /// <summary>Apply this reward to the radio station.</summary>
    public void Apply(RadioStation radio, DayEndHandler dayEndHandler = null)
    {
        switch (type)
        {
            case RewardType.ListenersBoost:
                // Flat listener gain
                if (listenersBoostFlat.totalListeners > 0)
                    radio.AddListeners(ToPercentage(listenersBoostFlat, radio));

                // Stacking modifier — uses Add, not Set, so rewards accumulate
                if (HasNonZeroModifier(listenersModifier))
                {
                    if (listenerModifierDuration == ModifierDuration.Daily)
                        radio.AddDailyListenersModifier(
                            listenersModifier.hipHop, listenersModifier.disco,
                            listenersModifier.rock,   listenersModifier.metal);
                    else
                        radio.AddHourlyListenersModifier(
                            listenersModifier.hipHop, listenersModifier.disco,
                            listenersModifier.rock,   listenersModifier.metal);

                    Debug.Log($"[Reward] Listener modifier applied ({listenerModifierDuration}).");
                }
                break;

            case RewardType.RevenueBoost:
                if (HasNonZeroModifier(revenueModifier))
                {
                    if (revenueModifierDuration == ModifierDuration.Daily)
                        radio.AddDailyRevenueModifier(
                            revenueModifier.hipHop, revenueModifier.disco,
                            revenueModifier.rock,   revenueModifier.metal);
                    else
                        radio.AddHourlyRevenueModifier(
                            revenueModifier.hipHop, revenueModifier.disco,
                            revenueModifier.rock,   revenueModifier.metal);

                    Debug.Log($"[Reward] Revenue modifier applied ({revenueModifierDuration}).");
                }
                break;

            case RewardType.Money:
                radio.AddMoney(moneyAmount);
                Debug.Log($"[Reward] +{moneyAmount}$ added.");
                break;

            case RewardType.Cassette:
                if (cassetteToAdd != null && dayEndHandler != null)
                {
                    dayEndHandler.AddCassetteToOffer(cassetteToAdd);
                    Debug.Log($"[Reward] Cassette '{cassetteToAdd.name}' added to offer.");
                }
                break;
        }

        Debug.Log($"[Reward] Applied: {rewardName}");
    }

    // ------------------------------------------------------------------
    // Helpers

    private GenreValues ToPercentage(GenreValues flat, RadioStation radio)
    {
        return new GenreValues
        {
            hipHop = radio.currentListeners.hipHop > 0 ? Mathf.RoundToInt((float)flat.hipHop / radio.currentListeners.hipHop * 100f) : 0,
            disco  = radio.currentListeners.disco  > 0 ? Mathf.RoundToInt((float)flat.disco  / radio.currentListeners.disco  * 100f) : 0,
            rock   = radio.currentListeners.rock   > 0 ? Mathf.RoundToInt((float)flat.rock   / radio.currentListeners.rock   * 100f) : 0,
            metal  = radio.currentListeners.metal  > 0 ? Mathf.RoundToInt((float)flat.metal  / radio.currentListeners.metal  * 100f) : 0,
        };
    }

    private bool HasNonZeroModifier(GenreValuesModifier m)
        => m.hipHop != 0 || m.disco != 0 || m.rock != 0 || m.metal != 0;
}

// ------------------------------------------------------------------

public enum RewardType
{
    ListenersBoost,
    RevenueBoost,
    Money,
    Cassette
}

public enum ModifierDuration
{
    /// <summary>Cleared at the next hour tick.</summary>
    Hourly,
    /// <summary>Cleared at the end of the day.</summary>
    Daily
}