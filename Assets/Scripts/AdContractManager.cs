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
                    rotation = go.transform.rotation
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
    /// Losuje 5 reklam z puli i wyświetla panel proceduralnego UI wyboru kontraktów.
    /// </summary>
    public void ShowContractSelection(Action onFinished)
    {
        onSelectionFinished = onFinished;
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null) gm.SetInputEnabled(false);

        currentDailyOffers.Clear();
        if (allAds == null || allAds.Count == 0)
        {
            Debug.LogError("[AdContractManager] Brak zdefiniowanych reklam w allAds! Upewnij się, że przeniosłeś zlecenia do folderu Resources/Zlecenia.");
            onFinished?.Invoke();
            return;
        }

        List<Ad> pool = new List<Ad>(allAds);
        int offersCount = Mathf.Min(5, pool.Count);
        for (int i = 0; i < offersCount; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            currentDailyOffers.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        BuildAndShowSelectionUI();
    }

    /// <summary>
    /// Proceduralne budowanie ciemnego panelu UI (w stylu DaySummaryScreen).
    /// </summary>
    private void BuildAndShowSelectionUI()
    {
        if (uiCanvas != null) Destroy(uiCanvas);

        uiCanvas = new GameObject("AdContractCanvas");
        var canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = uiCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        uiCanvas.AddComponent<GraphicRaycaster>();

        var ct = uiCanvas.transform;

        // Ciemne tło (Overlay)
        var overlay = MakeImage(ct, "Overlay", new Color(0f, 0f, 0f, 0.75f));
        StretchFull(overlay);

        // Główny panel
        var panel = MakeImage(ct, "Panel", new Color32(18, 22, 28, 255));
        SR(panel, 0.5f, 0.5f, 1100, 860, 0, 0);

        var border = MakeImage(panel.transform, "Border", new Color32(35, 50, 70, 255));
        SR(border, 0.5f, 0.5f, 1080, 840, 0, 0);
        border.GetComponent<RectTransform>().SetAsFirstSibling();

        // Tytuły
        var titleText = MakeText(ct, "Title", "ZLECENIA REKLAMOWE", 38, new Color32(220, 180, 50, 255)).GetComponent<TextMeshProUGUI>();
        titleText.fontStyle = FontStyles.Bold;
        SR(titleText.gameObject, 0.5f, 0.5f, 1000, 56, 0, 360);

        var subtitle = MakeText(ct, "Subtitle", "Wybierz dowolną ilość zleceń do wyemitowania w dniu dzisiejszym.", 18, new Color32(140, 160, 190, 255));
        SR(subtitle, 0.5f, 0.5f, 1000, 34, 0, 315);

        SR(MakeImage(ct, "Div0", new Color32(50, 65, 90, 255)), 0.5f, 0.5f, 1020, 2, 0, 290);

        adToggles.Clear();
        float y = 210;
        float step = 115;

        for (int i = 0; i < currentDailyOffers.Count; i++)
        {
            Ad ad = currentDailyOffers[i];
            float payout = CalculatePotentialPayout(ad);

            // Wysoce czytelny kontener (Karta zlecenia)
            var rowBox = MakeImage(ct, $"RowBox_{i}", new Color32(25, 31, 40, 255));
            SR(rowBox, 0.5f, 0.5f, 1020, 95, 0, y);
            rowBox.AddComponent<Outline>().effectColor = new Color32(42, 58, 80, 255);

            var rowTransform = rowBox.transform;

            // Zleceniodawca (Tekst mały, złoty na górze po lewej)
            var clientTxt = MakeText(rowTransform, "Client", $"ZLECENIODAWCA: {ad.GetAuthor().ToUpper()}", 14, new Color32(220, 180, 50, 255));
            SR(clientTxt, 0.5f, 0.5f, 550, 24, -200, 20);
            var tmpClient = clientTxt.GetComponent<TextMeshProUGUI>();
            tmpClient.alignment = TextAlignmentOptions.Left;
            tmpClient.fontStyle = FontStyles.Bold;

            // Tytuł reklamy (Duży, biały bold pod zleceniodawcą)
            var titleTxt = MakeText(rowTransform, "Title", ad.GetName(), 20, new Color32(255, 255, 255, 255));
            SR(titleTxt, 0.5f, 0.5f, 550, 36, -200, -12);
            var tmpTitle = titleTxt.GetComponent<TextMeshProUGUI>();
            tmpTitle.alignment = TextAlignmentOptions.Left;
            tmpTitle.fontStyle = FontStyles.Bold;

            // Kontener wypłaty (Zielona plakietka cenowa po prawej)
            var payoutTag = MakeImage(rowTransform, "PayoutTag", new Color32(12, 45, 25, 255));
            SR(payoutTag, 0.5f, 0.5f, 220, 50, 220, 0);
            payoutTag.AddComponent<Outline>().effectColor = new Color32(30, 90, 50, 255);

            var payoutTxt = MakeText(payoutTag.transform, "PayoutText", $"EST. ZAROBEK: {payout:F2}$", 16, new Color32(80, 220, 100, 255));
            StretchFull(payoutTxt);
            var tmpPayout = payoutTxt.GetComponent<TextMeshProUGUI>();
            tmpPayout.alignment = TextAlignmentOptions.Center;
            tmpPayout.fontStyle = FontStyles.Bold;

            // Kontener Checkboxa
            var toggleGO = new GameObject("Toggle");
            toggleGO.transform.SetParent(rowTransform, false);
            SR(toggleGO, 0.5f, 0.5f, 45, 45, 440, 0);

            var toggleBg = MakeImage(toggleGO.transform, "Background", new Color32(20, 25, 35, 255));
            StretchFull(toggleBg);
            toggleBg.AddComponent<Outline>().effectColor = new Color32(50, 70, 95, 255);

            var toggleCheck = MakeImage(toggleGO.transform, "Checkmark", new Color32(220, 180, 50, 255));
            SR(toggleCheck, 0.5f, 0.5f, 28, 28, 0, 0);

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = toggleBg.GetComponent<Image>();
            toggle.graphic = toggleCheck.GetComponent<Image>();
            toggle.isOn = false;

            adToggles.Add(toggle);

            y -= step;
        }

        // Bottom divider
        SR(MakeImage(ct, "DivBottom", new Color32(50, 65, 90, 255)), 0.5f, 0.5f, 1020, 2, 0, -290);

        // Przycisk akceptacji
        var acceptBtn = MakeButton(ct, "AcceptBtn", "AKCEPTUJ ZLECENIA");
        SR(acceptBtn, 0.5f, 0.5f, 320, 60, 0, -350);

        acceptBtn.GetComponent<Button>().onClick.AddListener(OnAcceptClicked);

        Time.timeScale = 0f;
    }

    private void OnAcceptClicked()
    {
        Time.timeScale = 1f;

        // Czyszczenie kaset z poprzedniego dnia na biurku, jeśli jakieś zostały
        ClearRemainingPhysicalAds();

        activeContracts.Clear();

        for (int i = 0; i < adToggles.Count; i++)
        {
            if (adToggles[i].isOn)
            {
                activeContracts.Add(currentDailyOffers[i]);
            }
        }

        Debug.Log($"[AdContractManager] Zaakceptowano {activeContracts.Count} zleceń reklamowych.");

        // Spawnowanie kaset
        SpawnActiveContractCassettes();

        if (uiCanvas != null) Destroy(uiCanvas);

        onSelectionFinished?.Invoke();
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

            // Sprawdzamy, czy mamy zarejestrowane pozycje z WojtekScene
            if (i < preplacedSpawnPoints.Count)
            {
                spawnPos = preplacedSpawnPoints[i].position;
                spawnRot = preplacedSpawnPoints[i].rotation;
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
        physicalAdObjects.Clear();

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

    #region UI Helpers

    void MakeHeaderLabel(Transform p, string name, string text, float ox, float oy)
    {
        var go = MakeText(p, name, text, 16, new Color32(80, 100, 140, 255));
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontStyle = FontStyles.Bold;
        SR(go, 0.5f, 0.5f, 260, 30, ox, oy);
    }

    GameObject MakeImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<Image>().color = color;
        return obj;
    }

    GameObject MakeText(Transform parent, string name, string text, int size, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return obj;
    }

    GameObject MakeButton(Transform parent, string name, string label)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        var img = obj.AddComponent<Image>();
        img.color = new Color32(30, 55, 90, 255);
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color32(50, 85, 130, 255);
        cb.pressedColor = new Color32(220, 180, 50, 255);
        btn.colors = cb;
        var lbl = MakeText(obj.transform, "Label", label, 22, new Color32(220, 190, 50, 255));
        lbl.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        SR(lbl, 0.5f, 0.5f, 280, 60, 0, 0);
        return obj;
    }

    void SR(GameObject obj, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = obj.GetComponent<RectTransform>();
        if (!rt) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    void StretchFull(GameObject obj)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    #endregion
}
