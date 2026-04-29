using UnityEngine;

/// <summary>
/// Definicja minigry: łączy prefab minigry z pulą nagród.
/// Twórz przez: prawym → Create → MiniGame/Definition
/// </summary>
[CreateAssetMenu(fileName = "NowaMinigra", menuName = "MiniGame/Definition")]
public class MiniGameDefinition : ScriptableObject
{
    [Header("Identyfikacja")]
    public string miniGameName = "Nazwa Minigry";
    [TextArea] public string description;

    [Header("Prefab minigry")]
    [Tooltip("Prefab z komponentem dziedziczącym BaseMiniGame")]
    public GameObject prefab;

    [Header("Pula nagród")]
    [Tooltip("true  → losuj z globalnej puli MiniGameSystem\n" +
             "false → losuj z własnej puli poniżej")]
    public bool useGlobalRewardPool = true;

    [Tooltip("Własne nagrody — aktywne tylko gdy useGlobalRewardPool = false")]
    public MiniGameReward[] ownRewards;

    [Header("Ile nagród dostaje gracz po wygranej")]
    [Min(1)] public int rewardsToGive = 1;

    // ------------------------------------------------------------------
    /// <summary>
    /// Losuje nagrody. Przekaż globalną pulę z MiniGameSystem.
    /// Jeśli useGlobalRewardPool = false, globalPool jest ignorowane.
    /// </summary>
    public MiniGameReward[] DrawRewards(MiniGameReward[] globalPool)
    {
        MiniGameReward[] pool = useGlobalRewardPool ? globalPool : ownRewards;

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning($"[{miniGameName}] Brak nagród w puli " +
                             $"({(useGlobalRewardPool ? "globalna" : "własna")})!");
            return new MiniGameReward[0];
        }

        int count = Mathf.Min(rewardsToGive, pool.Length);
        var result = new MiniGameReward[count];
        var used   = new bool[pool.Length];

        for (int i = 0; i < count; i++)
            result[i] = DrawOne(pool, used);

        return result;
    }

    // ------------------------------------------------------------------
    MiniGameReward DrawOne(MiniGameReward[] pool, bool[] used)
    {
        int totalWeight = 0;
        for (int i = 0; i < pool.Length; i++)
            if (!used[i] && pool[i] != null)
                totalWeight += pool[i].weight;

        if (totalWeight == 0) return null;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < pool.Length; i++)
        {
            if (used[i] || pool[i] == null) continue;
            cumulative += pool[i].weight;
            if (roll < cumulative)
            {
                used[i] = true;
                return pool[i];
            }
        }

        return pool[0];
    }
}
