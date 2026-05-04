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

    [Header("Config")]
    public List<PhoneCallDefinition> dialogPhoneCalls;
    
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

    protected override void OnLaunch()
    {
        // 50% chance for Song Request, 50% for Dialog Call (or configure it)
        bool isSongRequest = Random.value > 0.5f;

        if (isSongRequest && playerCassettesPool != null && playerCassettesPool.Count > 0)
        {
            SetupSongRequest();
        }
        else
        {
            SetupDialogCall();
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
    else if (requestedCassette.GetCassetteValues().metal > 0) genreText = "metal";
    
    string dialog = "";
    switch(genreText)
    {
        case "hip hop": dialog = $"Yo, macie może kasetę '{requestedCassette.GetName()}'? Puśćcie trochę hip hopu!"; break;
        case "disco": dialog = $"Hej! Zróbcie trochę hałasu i puśćcie '{requestedCassette.GetName()}'!"; break;
        case "rock": dialog = $"Witam. Chciałbym usłyszeć klasycznego rocka. Może '{requestedCassette.GetName()}'?"; break;
        case "metal": dialog = $"Dawajcie metal! Puść '{requestedCassette.GetName()}' natychmiast!"; break;
        default: dialog = $"Hej, puść proszę '{requestedCassette.GetName()}'."; break;
    }

    if (dialogText != null) dialogText.text = dialog;

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
    if (dialogPhoneCalls == null || dialogPhoneCalls.Count == 0)
    {
        Close();
        return;
    }

    requestedCassette = null;
    dynamicallyPickedGenre = "";
    currentPhoneCall = dialogPhoneCalls[Random.Range(0, dialogPhoneCalls.Count)];

    string dialogTextString = currentPhoneCall.initialDialog;
    if (dialogTextString != null && dialogTextString.Contains("{GENRE}"))
    {
        string[] genres = { "hip hop", "disco", "rock", "metal" };
        dynamicallyPickedGenre = genres[Random.Range(0, genres.Length)];
        dialogTextString = dialogTextString.Replace("{GENRE}", dynamicallyPickedGenre);
    }

    if (dialogText != null) dialogText.text = dialogTextString;

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

    private void OnOkayClicked()
    {
        Debug.Log($"Requested cassette: {requestedCassette?.GetName()}");
        
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
            radioStation.currentListeners.metal += option.flatListenersChange.metal;
        }
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
}
