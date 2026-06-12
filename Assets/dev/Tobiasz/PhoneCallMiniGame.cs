using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PhoneCallMiniGame : BaseMiniGame
{
    [Header("Phone Call UI")]
    public TextMeshProUGUI dialogText;
    
    [Header("Song Request UI")]
    public GameObject songRequestPanel;
    public Button okayButton; // For song request, there's only one choice.

    [Header("Dialog Options UI")]
    public GameObject dialogOptionsPanel;
    public Button optionAButton;
    public TextMeshProUGUI optionAText;
    public Button optionBButton;
    public TextMeshProUGUI optionBText;

    [Header("Typewriter Settings")]
    public float typeDelay = 0.01f;
    private Coroutine typingCoroutine;

    [Header("Animation Settings")]
    public float slideDuration = 0.4f;
    public float slideOffset = -400f; // animacja wsuwania od 400px nizej
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Vector2 optionAOriginalPos;
    private Vector2 optionBOriginalPos;
    private Vector2 okayOriginalPos;
    private bool originalPositionsSaved = false;
    private List<Button> activeButtonsToSlide = new List<Button>();

    [Header("Config")]
    public List<PhoneCallDefinition> dialogPhoneCalls;
    public FMODUnity.EventReference phoneRingSound;
    public FMODUnity.EventReference phonePickUpSound;
    public FMODUnity.EventReference phonePutDownSound;
    public FMODUnity.EventReference clickOptionSound;
    
    // Zastępstwo dla ekwipunku gracza
    public List<Cassette> playerCassettesPool;

    // References to other systems to apply rewards/penalties
    private RadioStation radioStation;
    private GameManager gameManager;

        private string dynamicallyPickedGenre = "";
private PlayableContent requestedCassette;
    private PhoneCallDefinition currentPhoneCall;

private void Start()
{
    gameManager = FindFirstObjectByType<GameManager>();
    if (gameManager != null)
    {
        radioStation = gameManager.radioStation;
    }
}

    // Pula telefonów bez powtórzeń — raz wylosowany telefon znika z niej do końca gry.
    private List<PhoneCallDefinition> remainingDialogCalls;

    // Inicjuje pulę przy pierwszym użyciu (kopia konfiguracji — nie ruszamy oryginału).
    private void EnsurePool()
    {
        if (remainingDialogCalls == null)
            remainingDialogCalls = dialogPhoneCalls != null
                ? new List<PhoneCallDefinition>(dialogPhoneCalls)
                : new List<PhoneCallDefinition>();
    }

    protected override void OnLaunch()
    {   
        FMODUnity.RuntimeManager.PlayOneShot(phoneRingSound, this.transform.position);
        FMODUnity.RuntimeManager.PlayOneShot(phonePickUpSound, this.transform.position);
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        // Na czas rozmowy blokuj interakcję ze światem (wkładanie/wyjmowanie kaset).
        if (gameManager != null) gameManager.SetInputEnabled(false);

        EnsurePool();

        // Najpierw ZAWSZE telefony narracyjne; prośby o piosenkę dopiero gdy się skończą.
        if (remainingDialogCalls.Count > 0)
        {
            SetupDialogCall();
        }
        else if (playerCassettesPool != null && playerCassettesPool.Count > 0)
        {
            SetupSongRequest();
        }
        else
        {
            Close();
        }
    }

private void SetupSongRequest()
{
    currentPhoneCall = null;
    requestedCassette = playerCassettesPool[Random.Range(0, playerCassettesPool.Count)];
    
    string genreText = "";
    if (requestedCassette.GetCassetteValues().hipHop > 0) genreText = "hip hop";
    else if (requestedCassette.GetCassetteValues().disco > 0) genreText = "disco";
    else if (requestedCassette.GetCassetteValues().rock > 0) genreText = "rock";
    else if (requestedCassette.GetCassetteValues().pop > 0) genreText = "pop";
    
    string dialog = "";
    switch(genreText)
    {
        case "hip hop": dialog = $"Yo, macie może kasetę '{requestedCassette.GetName()}'? Puśćcie trochę hip hopu!"; break;
        case "disco": dialog = $"Hej! Zróbcie trochę hałasu i puśćcie '{requestedCassette.GetName()}'!"; break;
        case "rock": dialog = $"Witam. Chciałbym usłyszeć klasycznego rocka. Może '{requestedCassette.GetName()}'?"; break;
        case "pop": dialog = $"Dawajcie pop! Puść '{requestedCassette.GetName()}' natychmiast!"; break;
        default: dialog = $"Hej, puść proszę '{requestedCassette.GetName()}'."; break;
    }

    if (dialogText != null) StartTyping(dialog);

    if (optionAButton != null)
    {
        optionAButton.gameObject.SetActive(true);
        optionAButton.onClick.RemoveAllListeners();
        optionAButton.onClick.AddListener(OnOkayClicked);
        if (optionAText != null) optionAText.text = "OK";
    }
    if (optionBButton != null) optionBButton.gameObject.SetActive(false);
}

private void SetupDialogCall()
{

   
    EnsurePool();

    if (remainingDialogCalls.Count == 0)
    {
        // Brak nieużytych telefonów — jeśli są kasety, zrób prośbę o piosenkę, inaczej zamknij.
        if (playerCassettesPool != null && playerCassettesPool.Count > 0)
        {
            SetupSongRequest();
            return;
        }
        Close();
        return;
    }

    requestedCassette = null;
    dynamicallyPickedGenre = "";
    int idx = Random.Range(0, remainingDialogCalls.Count);
    currentPhoneCall = remainingDialogCalls[idx];
    remainingDialogCalls.RemoveAt(idx); // bez powtórzeń

    string dialogTextString = currentPhoneCall.initialDialog;
    if (dialogTextString != null && dialogTextString.Contains("{GENRE}"))
    {
        string[] genres = { "hip hop", "disco", "rock", "pop" };
        dynamicallyPickedGenre = genres[Random.Range(0, genres.Length)];
        dialogTextString = dialogTextString.Replace("{GENRE}", dynamicallyPickedGenre);
    }

    if (dialogText != null) StartTyping(dialogTextString);

    if (optionAButton != null)
    {
        optionAButton.onClick.RemoveAllListeners();
        optionAButton.onClick.AddListener(() => OnOptionClicked(0));
        if (currentPhoneCall.dialogOptions.Count > 0)
        {
            optionAButton.gameObject.SetActive(true);
            if (optionAText != null) optionAText.text = currentPhoneCall.dialogOptions[0].optionText;
        }
        else
        {
            optionAButton.gameObject.SetActive(false);
        }
    }

    if (optionBButton != null)
    {
        optionBButton.onClick.RemoveAllListeners();
        optionBButton.onClick.AddListener(() => OnOptionClicked(1));
        if (currentPhoneCall.dialogOptions.Count > 1)
        {
            optionBButton.gameObject.SetActive(true);
            if (optionBText != null) optionBText.text = currentPhoneCall.dialogOptions[1].optionText;
        }
        else
        {
            optionBButton.gameObject.SetActive(false);
        }
    }
}

    private void SaveOriginalPositions()
    {
        if (originalPositionsSaved) return;
        if (optionAButton != null) optionAOriginalPos = optionAButton.GetComponent<RectTransform>().anchoredPosition;
        if (optionBButton != null) optionBOriginalPos = optionBButton.GetComponent<RectTransform>().anchoredPosition;
        if (okayButton != null) okayOriginalPos = okayButton.GetComponent<RectTransform>().anchoredPosition;
        originalPositionsSaved = true;
    }

    private Vector2 GetOriginalPos(Button btn)
    {
        if (btn == optionAButton) return optionAOriginalPos;
        if (btn == optionBButton) return optionBOriginalPos;
        if (btn == okayButton) return okayOriginalPos;
        return Vector2.zero;
    }

    private void StartTyping(string text)
    {
        SaveOriginalPositions();
        activeButtonsToSlide.Clear();

        if (optionAButton != null && optionAButton.gameObject.activeSelf) activeButtonsToSlide.Add(optionAButton);
        if (optionBButton != null && optionBButton.gameObject.activeSelf) activeButtonsToSlide.Add(optionBButton);
        if (okayButton != null && okayButton.gameObject.activeSelf) activeButtonsToSlide.Add(okayButton);

        foreach (var btn in activeButtonsToSlide)
        {
            btn.GetComponent<RectTransform>().anchoredPosition = GetOriginalPos(btn) + new Vector2(0, slideOffset);
            btn.interactable = false;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        dialogText.text = text;
        dialogText.maxVisibleCharacters = 0;
        
        typingCoroutine = StartCoroutine(TypewriterEffect());
    }

    private System.Collections.IEnumerator TypewriterEffect()
    {
        dialogText.ForceMeshUpdate();
        int totalVisibleCharacters = dialogText.textInfo.characterCount;
        int counter = 0;

        while (counter < totalVisibleCharacters)
        {
            counter++;
            dialogText.maxVisibleCharacters = counter;
            yield return new WaitForSecondsRealtime(typeDelay);
        }

        // animacja przyciskow
        float t = 0;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(t / slideDuration);
            float curveValue = slideCurve.Evaluate(normalizedTime);

            foreach (var btn in activeButtonsToSlide)
            {
                var rt = btn.GetComponent<RectTransform>();
                Vector2 origPos = GetOriginalPos(btn);
                Vector2 startPos = origPos + new Vector2(0, slideOffset);
                rt.anchoredPosition = Vector2.LerpUnclamped(startPos, origPos, curveValue);
            }
            yield return null;
        }

        foreach (var btn in activeButtonsToSlide)
        {
            btn.GetComponent<RectTransform>().anchoredPosition = GetOriginalPos(btn);
            btn.interactable = true;
        }
    }

    private void OnOkayClicked()
    {
        Debug.Log($"Requested cassette: {requestedCassette?.GetName()}");
        FMODUnity.RuntimeManager.PlayOneShot(clickOptionSound, this.transform.position);
        if (gameManager != null && requestedCassette != null)
        {
            gameManager.SetRequestedCassette(requestedCassette);
        }
        
        TriggerWin();
        Close();
    }

private void OnOptionClicked(int optionIndex)
{
    
    if (currentPhoneCall == null || optionIndex >= currentPhoneCall.dialogOptions.Count) return;

    PhoneCallDialogOption option = currentPhoneCall.dialogOptions[optionIndex];

    if (radioStation != null)
    {
        radioStation.AddMoney(option.moneyChange);
        
        if (option.listenersPrecentageChange != 0f)
        {
            radioStation.RemoveListenersPr(-option.listenersPrecentageChange);
        }

        if (option.flatListenersChange.totalListeners != 0)
        {
            radioStation.currentListeners.hipHop += option.flatListenersChange.hipHop;
            radioStation.currentListeners.disco += option.flatListenersChange.disco;
            radioStation.currentListeners.rock += option.flatListenersChange.rock;
            radioStation.currentListeners.pop += option.flatListenersChange.pop;
        }
    }

    // Wpływ wyboru na endingowe liczniki (osie zakończenia gry)
    if (gameManager != null)
    {
        if (option.hostDelta != 0)       gameManager.ApplyEndingMeter(EndingMeter.Host, option.hostDelta);
        if (option.listenerDelta != 0)   gameManager.ApplyEndingMeter(EndingMeter.Listener, option.listenerDelta);
        if (option.governmentDelta != 0) gameManager.ApplyEndingMeter(EndingMeter.Government, option.governmentDelta);
    }

    string genreToSend = option.requestedGenre;
    if (genreToSend == "random" && !string.IsNullOrEmpty(dynamicallyPickedGenre))
    {
        genreToSend = dynamicallyPickedGenre;
    }

    if (!string.IsNullOrEmpty(genreToSend) && gameManager != null)
    {
        gameManager.SetRequestedGenre(genreToSend);
    }

    Debug.Log($"Dialog resulting text: {option.resultingText}");

    TriggerWin();
    Close();
}

    public override void Close()
    {   
        FMODUnity.RuntimeManager.PlayOneShot(phonePutDownSound, this.transform.position);
        // Po zakończeniu rozmowy przywróć sterowanie (wkładanie kaset itd.).
        if (gameManager != null) gameManager.SetInputEnabled(true);
        base.Close();
    }
}
