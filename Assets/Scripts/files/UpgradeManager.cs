using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zarządza stanem ulepszeń gracza (jednorazowy zakup każdego ulepszenia)
/// oraz losowaniem dziennej oferty draftu. Samo UI buduje
/// <see cref="DaySummarySceneManager"/> w stylu ekranu podsumowania dnia.
///
/// SETUP:
///   1. Komponent na obiekcie w scenie gry (singleton).
///   2. Przypisz allUpgrades[] — wszystkie SO ulepszeń.
///
/// PRZEPŁYW:
///   - DaySummarySceneManager.GetOrCreateDraftOptions() — pobiera dzienną ofertę.
///   - DaySummarySceneManager wywołuje TryPurchase() na klik "KUP".
///   - Po przejściu dalej ClearDraftOptions() resetuje ofertę na następny dzień.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // ------------------------------------------------------------------
    [Header("Pula wszystkich ulepszeń")]
    public UpgradeDefinition[] allUpgrades;

    [Header("Ile opcji w drafcie")]
    public int draftChoices = 3;

    // Tylko te typy są dostępne do kupienia w sklepie ulepszeń.
    private static readonly UpgradeType[] DraftableTypes =
    {
        UpgradeType.DiscoNight,
        UpgradeType.PopStars,
        UpgradeType.ComptonVibes,
        UpgradeType.RockAndRoll,
        UpgradeType.NewHorizons
    };

    // ------------------------------------------------------------------
    // Stan — każde ulepszenie można kupić tylko raz.
    private readonly HashSet<UpgradeType> ownedTypes = new HashSet<UpgradeType>();

    // Dzienna oferta draftu (cache, żeby przebudowa panelu nie losowała od nowa).
    private List<UpgradeDefinition> currentDraftOptions;

    // Back2Back — czy już użyto w tej godzinie (typ pozostaje w kodzie, ale nie jest kupowalny)
    private bool back2BackUsedThisHour = false;
    private bool luckyDrawAvailable = false;

    // ------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ------------------------------------------------------------------
    #region Public API

    /// <summary>Czy gracz posiada dane ulepszenie.</summary>
    public bool HasUpgrade(UpgradeType type) => ownedTypes.Contains(type);

    // --- Back2Back (zachowane w kodzie) ---
    public bool TryConsumeBack2Back()
    {
        if (!HasUpgrade(UpgradeType.Back2Back)) return false;
        if (back2BackUsedThisHour) return false;
        back2BackUsedThisHour = true;
        return true;
    }

    public void ResetBack2Back() => back2BackUsedThisHour = false;

    // --- LuckyDraw (zachowane w kodzie) ---
    public bool HasLuckyDraw() => luckyDrawAvailable;
    public void ConsumeLuckyDraw() => luckyDrawAvailable = false;

    // --- MarketFlood (zachowane w kodzie; nie jest kupowalny więc zwraca 0) ---
    public int GetExtraShopSlots()
    {
        if (!HasUpgrade(UpgradeType.MarketFlood)) return 0;
        foreach (var def in allUpgrades)
            if (def != null && def.type == UpgradeType.MarketFlood)
                return def.marketFloodExtraSlots;
        return 0;
    }

    /// <summary>
    /// Dzienna oferta ulepszeń (do <see cref="draftChoices"/> losowych, jeszcze nieposiadanych).
    /// Losowana raz dziennie i cache'owana aż do <see cref="ClearDraftOptions"/>.
    /// </summary>
    public List<UpgradeDefinition> GetOrCreateDraftOptions()
    {
        if (currentDraftOptions == null)
            currentDraftOptions = DrawDraftOptions();
        return currentDraftOptions;
    }

    /// <summary>Resetuje dzienną ofertę — wywołaj po zamknięciu sklepu ulepszeń.</summary>
    public void ClearDraftOptions() => currentDraftOptions = null;

    /// <summary>
    /// Próba zakupu ulepszenia: sprawdza posiadanie i stan konta, odejmuje kasę,
    /// rejestruje i aplikuje trwały efekt. Zwraca true przy powodzeniu.
    /// </summary>
    public bool TryPurchase(UpgradeDefinition upgrade)
    {
        if (upgrade == null || HasUpgrade(upgrade.type)) return false;

        var gm = FindFirstObjectByType<GameManager>();
        if (gm == null || gm.radioStation == null) return false;

        RadioStation radio = gm.radioStation;
        if (radio.GetCurrentMoney() < upgrade.cost) return false;

        radio.SetCurrentMoney(radio.GetCurrentMoney() - upgrade.cost);
        ownedTypes.Add(upgrade.type);
        ApplyUpgrade(upgrade, radio, gm);

        Debug.Log($"[UpgradeManager] Zakupiono: {upgrade.upgradeName} za {upgrade.cost}$");
        return true;
    }

    #endregion

    // ------------------------------------------------------------------
    #region Draft Logic

    private bool IsDraftable(UpgradeDefinition def) =>
        def != null && Array.IndexOf(DraftableTypes, def.type) >= 0;

    private List<UpgradeDefinition> DrawDraftOptions()
    {
        var result = new List<UpgradeDefinition>();
        if (allUpgrades == null || allUpgrades.Length == 0) return result;

        // Pula: tylko kupowalne typy, jeszcze nieposiadane, każdy typ raz.
        var pool = new List<UpgradeDefinition>();
        var seen = new HashSet<UpgradeType>();
        foreach (var u in allUpgrades)
        {
            if (!IsDraftable(u)) continue;
            if (HasUpgrade(u.type)) continue;
            if (!seen.Add(u.type)) continue;
            pool.Add(u);
        }

        int count = Mathf.Min(draftChoices, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int totalWeight = 0;
            foreach (var u in pool) totalWeight += Mathf.Max(1, u.weight);

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumul = 0;
            UpgradeDefinition picked = pool[0];

            foreach (var u in pool)
            {
                cumul += Mathf.Max(1, u.weight);
                if (roll < cumul) { picked = u; break; }
            }

            result.Add(picked);
            pool.Remove(picked);
        }

        return result;
    }

    #endregion

    // ------------------------------------------------------------------
    #region Apply Logic

    private void ApplyUpgrade(UpgradeDefinition upgrade, RadioStation radio, GameManager gm)
    {
        switch (upgrade.type)
        {
            // -- Trwały bonus do bazowego mnożnika danej kategorii słuchaczy --
            // genreBonus=10 -> +0.10 do mnożnika (z 1.0 do 1.10).
            case UpgradeType.DiscoNight:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddUpgradeListenersModifier(0, bonus, 0, 0);
                Debug.Log($"[Upgrade] DiscoNight — +{upgrade.genreBonus} do bazowego mnożnika Disco.");
                break;
            }
            case UpgradeType.PopStars:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddUpgradeListenersModifier(0, 0, 0, bonus);
                Debug.Log($"[Upgrade] PopStars — +{upgrade.genreBonus} do bazowego mnożnika Pop.");
                break;
            }
            case UpgradeType.ComptonVibes:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddUpgradeListenersModifier(bonus, 0, 0, 0);
                Debug.Log($"[Upgrade] ComptonVibes — +{upgrade.genreBonus} do bazowego mnożnika HipHop.");
                break;
            }
            case UpgradeType.RockAndRoll:
            {
                float bonus = upgrade.genreBonus * 0.01f;
                radio.AddUpgradeListenersModifier(0, 0, bonus, 0);
                Debug.Log($"[Upgrade] RockAndRoll — +{upgrade.genreBonus} do bazowego mnożnika Rock.");
                break;
            }

            // -- Nowe Horyzonty: trwały bonus do każdej kategorii (newHorizonsBonus=5 -> +0.05) --
            case UpgradeType.NewHorizons:
            {
                float bonus = upgrade.newHorizonsBonus * 0.01f;
                radio.AddUpgradeListenersModifier(bonus, bonus, bonus, bonus);
                Debug.Log($"[Upgrade] NewHorizons — +{upgrade.newHorizonsBonus} do bazowego mnożnika każdej kategorii.");
                break;
            }

            // -- Typy zachowane w kodzie (nie są kupowalne w obecnej puli) --
            case UpgradeType.Back2Back:
                break;
            case UpgradeType.RollBack:
                ApplyRollBack(gm);
                break;
            case UpgradeType.LuckyDraw:
                luckyDrawAvailable = true;
                break;
            case UpgradeType.MarketFlood:
                break;
        }
    }

    /// <summary>
    /// RollBack — kasuje negatywne ostatnie wartości kaset (przywraca oryginalne).
    /// </summary>
    private void ApplyRollBack(GameManager gm)
    {
        var playables = FindObjectsByType<PlayableObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var p in playables)
        {
            if (p.data != null && p.data.GetType() == CassetteTypes.Music && p.data.GetTimesUsed() > 0)
            {
                p.data.ResetTimesUsed();
                p.data.ResetLastValues();
                count++;
            }
        }
        Debug.Log($"[Upgrade] RollBack — zresetowano {count} kaset.");
    }

    #endregion
}
