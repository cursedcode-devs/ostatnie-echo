using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdContractManager : MonoBehaviour
{
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    public struct UnplayedAdPenalty
    {
        public string clientName;
        public string adTitle;
        public float penaltyAmount;
    }

    [Header("Baza Danych Reklam")]
    [Tooltip("Lista wszystkich 16 predefiniowanych ScriptableObject reklam")]
    public List<Ad> allAds = new List<Ad>();

    [Header("Ustawienia Prefabów")]
    [Tooltip("Prefab fizycznej kasety reklamowej (np. Reklama.prefab)")]
    public GameObject adCassettePrefab;
    public Material adMaterial;

    [Header("Stan Systemu")]
    [SerializeField] private List<Ad> activeContracts = new List<Ad>();
    [SerializeField] private List<Ad> currentDailyOffers = new List<Ad>();
    [SerializeField] private List<TransformData> preplacedSpawnPoints = new List<TransformData>();

    private GameObject uiCanvas;
    private Action onSelectionFinished;
    private List<Toggle> adToggles = new List<Toggle>();
    
    // Lista fizycznie zespawnowanych kaset w danym dniu
    private Dictionary<Ad, GameObject> physicalAdObjects = new Dictionary<Ad, GameObject>();

    private void Start()
    {
        // 1. Znajdujemy i zapamiętujemy predefiniowane punkty spawnu ze sceny WojtekScene
        FindPreplacedSpawnPoints();

        // 2. Ładujemy wszystkie reklamy z folderu Resources/Zlecenia
        var loadedAds = Resources.LoadAll<Ad>("Zlecenia");
        if (loadedAds != null && loadedAds.Length > 0)
        {
            allAds.Clear();
            allAds.AddRange(loadedAds);
            Debug.Log($"[AdContractManager] Pomyślnie załadowano {allAds.Count} nowych zleceń reklamowych z Resources/Zlecenia.");
        }
        else
        {
            var backupAds = Resources.FindObjectsOfTypeAll<Ad>();
            if (backupAds != null && backupAds.Length > 0)
            {
                allAds.Clear();
                allAds.AddRange(backupAds);
                Debug.Log($"[AdContractManager] Wczytano {allAds.Count} kaset za pomocą FindObjectsOfTypeAll.");
            }
        }

        // Automatyczne dopasowanie prefabu
        if (adCassettePrefab == null)
        {
            adCassettePrefab = Resources.Load<GameObject>("Prefabs/Reklama");
            if (adCassettePrefab == null)
            {
                adCassettePrefab = Resources.Load<GameObject>("Reklama");
            }
        }
    }

    /// <summary>
    /// Znajduje i zapisuje pozycje predefiniowanych reklam umieszczonych na biurku na lewo.
    /// Następnie ukrywa je, aby gracz nie widział placeholderów przed zaakceptowaniem ofert.
    /// </summary>
    private void FindPreplacedSpawnPoints()
    {
        preplacedSpawnPoints.Clear();

        string[] targetNames = new string[] {
            "KasetaReklama",
            "KasetaReklama1",
            "KasetaReklama2",
            "KasetaReklama3",
            "KasetaReklama4"
        };


        foreach (string name in targetNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                preplacedSpawnPoints.Add(new TransformData {
                    position = go.transform.position,
                    rotation = go.transform.rotation,
                    scale = go.transform.localScale
                });
                
                // Ukrywamy predefiniowany placeholder ze sceny
                go.SetActive(false);
                Debug.Log($"[AdContractManager] Zarejestrowano i ukryto placeholder zlecenia: '{go.name}' na pozycji {go.transform.position}");
            }
            else
            {
                Debug.LogWarning($"[AdContractManager] Nie znaleziono obiektu '{name}' na scenie WojtekScene.");
            }
        }
    }

    /// <summary>
    /// Oblicza potencjalną zapłatę za wyemitowanie danej reklamy w tym momencie.
    /// </summary>
    public float CalculatePotentialPayout(Ad ad)
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null || gm.radioStation == null) return 0f;

        RadioStation radio = gm.radioStation;
        GenreValues values = ad.GetCassetteValues();
        GenreValues currentListeners = radio.currentListeners;
        GenreValuesModifier totalRevenueModifier = radio.GetTotalRevenueModifier();

        float hipHopRev = (values.hipHop / 100f) * currentListeners.hipHop * totalRevenueModifier.hipHop;
        float discoRev = (values.disco / 100f) * currentListeners.disco * totalRevenueModifier.disco;
        float rockRev = (values.rock / 100f) * currentListeners.rock * totalRevenueModifier.rock;
        float popRev = (values.pop / 100f) * currentListeners.pop * totalRevenueModifier.pop;

        return hipHopRev + discoRev + rockRev + popRev;
    }

    /// <summary>
    /// Generuje i zwraca listę ofert na dany dzień, bez wyświetlania UI.
    /// </summary>
    public List<Ad> GenerateDailyOffers(int count = 5)
    {
        currentDailyOffers.Clear();
        if (allAds == null || allAds.Count == 0)
        {
            Debug.LogError("[AdContractManager] Brak zdefiniowanych reklam w allAds!");
            return currentDailyOffers;
        }

        List<Ad> pool = new List<Ad>(allAds);
        int offersCount = Mathf.Min(count, pool.Count);
        for (int i = 0; i < offersCount; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            currentDailyOffers.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return currentDailyOffers;
    }

    /// <summary>
    /// Akceptuje wybrane reklamy z zewnętrznego UI i spawnuje kasety.
    /// </summary>
    public void AcceptContracts(List<Ad> selectedAds)
    {
        ClearRemainingPhysicalAds();
        activeContracts.Clear();
        activeContracts.AddRange(selectedAds);
        
        Debug.Log($"[AdContractManager] Zaakceptowano {activeContracts.Count} zleceń reklamowych z nowego UI.");
        SpawnActiveContractCassettes();
    }
/// <summary>
    /// Spawnuje fizyczne kasety reklamowe na biurku w pozycjach predefiniowanych w edytorze.
    /// </summary>
    private void SpawnActiveContractCassettes()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        GameObject prefab = adCassettePrefab;
        if (prefab == null)
        {
            var playables = FindObjectsByType<PlayableObject>(FindObjectsSortMode.None);
            if (playables != null && playables.Length > 0)
            {
                prefab = playables[0].gameObject;
            }
        }

        if (prefab == null)
        {
            Debug.LogError("[AdContractManager] Nie znaleziono żadnego prefabu do zespawnowania kasety!");
            return;
        }

        physicalAdObjects.Clear();
        List<Ad> clonedInstances = new List<Ad>();

        for (int i = 0; i < activeContracts.Count; i++)
        {
            Ad adData = activeContracts[i];
            
            Vector3 spawnPos;
            Quaternion spawnRot;
            Vector3 spawnScale = Vector3.one;

            // Sprawdzamy, czy mamy zarejestrowane pozycje z WojtekScene
            if (i < preplacedSpawnPoints.Count)
            {
                spawnPos = preplacedSpawnPoints[i].position;
                spawnRot = preplacedSpawnPoints[i].rotation;
                spawnScale = preplacedSpawnPoints[i].scale;
                Debug.Log($"[AdContractManager] Użycie pozycji ze sceny dla nowej reklamy #{i}: {spawnPos}");
            }
            else
            {
                // Fallback na wypadek gdyby gracz wybrał więcej reklam niż predefiniowano (np. powyżej 5)
                Vector3 basePos = Vector3.zero;
                if (gm.cassetteSlots != null && gm.cassetteSlots.Length > 0 && gm.cassetteSlots[0] != null)
                {
                    basePos = gm.cassetteSlots[0].transform.position + new Vector3(0.5f, -0.15f, -0.3f);
                }
                else
                {
                    basePos = new Vector3(0.2f, 0.1f, -1.0f);
                }
                Vector3 offset = new Vector3((i % 3) * 0.22f, 0.02f, (i / 3) * 0.18f);
                spawnPos = basePos + offset;
                spawnRot = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(-15f, 15f), 0f));
                Debug.Log($"[AdContractManager] Użycie pozycji fallback dla reklamy #{i}: {spawnPos}");
            }

            // Klonujemy prefab
            GameObject cassetteGo = Instantiate(prefab, spawnPos, spawnRot);
            cassetteGo.GetComponent<MeshRenderer>().material = adMaterial;            
            cassetteGo.transform.localScale = spawnScale;
            cassetteGo.name = $"Reklama_{adData.GetName()}";
            cassetteGo.SetActive(true);

            // Kluczowe: Klonujemy ScriptableObject Ad, by nie modyfikować oryginalnego pliku assetu
            Ad adInstance = Instantiate(adData);
            adInstance.ResetTimesUsed();
            adInstance.ResetLastValues();

            // Przypisanie komponentu PlayableObject
            var playableObj = cassetteGo.GetComponent<PlayableObject>();
            if (playableObj == null)
            {
                playableObj = cassetteGo.AddComponent<PlayableObject>();
            }

            playableObj.data = adInstance;

            // Ustawienie Hover UI
            var hoverUI = GameObject.Find("CassetteHoverScript");
            if (hoverUI != null)
            {
                playableObj.hoverUIScriptObject = hoverUI;
                playableObj.hoverUIScript = hoverUI.GetComponent<CassetteHoverUI>();
            }

            cassetteGo.tag = "Playable";

            physicalAdObjects[adInstance] = cassetteGo;
            clonedInstances.Add(adInstance);
        }

        activeContracts = clonedInstances;
    }

    /// <summary>
    /// Wywoływane z GameManager po odtworzeniu segmentu (Enter).
    /// Niszczy fizyczne kasety reklamowe, które zostały włożone do slotów i wyemitowane.
    /// </summary>
    public void HandleAdsPlayed(PlayableContent[] playedCassettes, CassetteSlotHandler[] slots)
    {
        GameManager gm = FindFirstObjectByType<GameManager>();

        for (int i = 0; i < slots.Length; i++)
        {
            var playableContent = playedCassettes[i];
            if (playableContent != null && playableContent.GetType() == CassetteTypes.Ad)
            {
                Ad adInstance = playableContent as Ad;
                GameObject physGo = null;

                // Szukamy po instancji w naszym słowniku
                foreach (var pair in physicalAdObjects)
                {
                    if (pair.Key.GetName() == adInstance.GetName() && pair.Key.GetAuthor() == adInstance.GetAuthor())
                    {
                        physGo = pair.Value;
                        break;
                    }
                }

                // Fallback: jeśli nie znaleźliśmy w słowniku, pobieramy bezpośrednio z CassetteSlotHandler
                if (physGo == null)
                {
                    var reflectionField = slots[i].GetType().GetField("cassette", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (reflectionField != null)
                    {
                        physGo = (GameObject)reflectionField.GetValue(slots[i]);
                    }
                }

                if (physGo != null)
                {
                    Debug.Log($"[AdContractManager] Niszczenie wyemitowanej fizycznej kasety reklamy: {playableContent.GetName()}");

                    if (gm != null && gm.selectionHandler != null)
                    {
                        if (gm.selectionHandler.GetSelectedObject() == physGo)
                        {
                            gm.selectionHandler.DeselectedObject(false, false);
                        }
                        if (gm.selectionHandler.GetLastSelectedObject() == physGo)
                        {
                            gm.selectionHandler.ResetLastSelectedObject();
                        }
                    }

                    Destroy(physGo);
                }

                var internalCassetteField = slots[i].GetType().GetField("cassette", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (internalCassetteField != null)
                {
                    internalCassetteField.SetValue(slots[i], null);
                }
                
                gm.airtime.emptySlot(i);
            }
        }
    }

    /// <summary>
    /// Wyliczanie kar na koniec dnia za KAŻDĄ zaakceptowaną reklamę, która NIE została wyemitowana.
    /// Nakłada karę w wysokości 1/2 potencjalnego zarobku i odejmuje ją z budżetu gracza.
    /// </summary>
    [Header("Listy Kar")]
    public List<UnplayedAdPenalty> lastDayPenalties = new List<UnplayedAdPenalty>();

    public float CalculateAndApplyPenalties()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null || gm.radioStation == null) return 0f;

        float totalPenalty = 0f;
        lastDayPenalties.Clear();

        foreach (var ad in activeContracts)
        {
            if (ad.GetTimesUsed() == 0)
            {
                float potentialEarn = CalculatePotentialPayout(ad);
                float penalty = potentialEarn * 0.5f;
                totalPenalty += penalty;
                
                lastDayPenalties.Add(new UnplayedAdPenalty {
                    clientName = ad.GetAuthor(),
                    adTitle = ad.GetName(),
                    penaltyAmount = penalty
                });
                
                Debug.Log($"[AdContractManager] Reklama '{ad.GetName()}' nie została wyemitowana! Kara: {penalty:F2}$ (1/2 z {potentialEarn:F2}$)");
            }
            else
            {
                // Reklama została pomyślnie wyemitowana (wzięta i wykonana).
                // Usuwamy ją z ogólnej puli reklam w allAds, aby nie pojawiła się w następne dni.
                allAds.RemoveAll(x => x.GetName() == ad.GetName() && x.GetAuthor() == ad.GetAuthor());
                Debug.Log($"[AdContractManager] Reklama '{ad.GetName()}' została wyemitowana. Usuwanie z puli dostępnych reklam.");
            }
        }

        if (totalPenalty > 0)
        {
            float currentMoney = gm.radioStation.GetCurrentMoney();
            gm.radioStation.SetCurrentMoney(currentMoney - totalPenalty);
            Debug.Log($"[AdContractManager] Łączna kara za reklamy: -{totalPenalty:F2}$. Zaktualizowano budżet do: {gm.radioStation.GetCurrentMoney():F2}$");
        }

        activeContracts.Clear();
        ClearRemainingPhysicalAds();

        return totalPenalty;
    }

    private void ClearRemainingPhysicalAds()
    {
        foreach (var pair in physicalAdObjects)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        physicalAdObjects.Clear();
    }
}


