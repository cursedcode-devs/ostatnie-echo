using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.VisualScripting.Member;
public class AudioQueueManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    public GameObject NowPlayingBar;
    /// <summary>
    /// Element kolejki: klip audio + opcjonalna zawartość kasety (np. reklama),
    /// dzięki czemu wiemy, kiedy podczas emisji pokazać napisy reklamy.
    /// </summary>
    private struct QueuedClip
    {
        public AudioClip clip;
        public PlayableContent content;

        public QueuedClip(AudioClip clip, PlayableContent content)
        {
            this.clip = clip;
            this.content = content;
        }
    }
    private TimeHandler timeHandler;
    private Queue<QueuedClip> audioQueue = new Queue<QueuedClip>();
    private bool isPlayingQueue = false;
    private bool skippedClip = false;
    public float musicVolume = 0.9f;
    public System.Action onSequenceFinished;

    public void SetTimeHandler(TimeHandler timeHandler)
    {
        this.timeHandler = timeHandler;
    }

    public bool IsPlaying()
    {
        return isPlayingQueue;
    }

    public void SkipSong()
    {
        if (!isPlayingQueue)
            return;

        skippedClip = true;
        audioSource.Stop();
        HideAdSubtitles();
    }

    public void PlayClipsSequence()
    {
        if (!isPlayingQueue)
        {
            isPlayingQueue = true;
            StartCoroutine(ProcessAudioQueue());
        }
    }

    public void EnqueueClips(AudioClip[] clipsToPlay)
    {
        foreach (AudioClip clip in clipsToPlay)
        {
            audioQueue.Enqueue(new QueuedClip(clip, null));
        }
    }

    public void EnqueueClip(AudioClip clip)
    {
        audioQueue.Enqueue(new QueuedClip(clip, null));
    }

    /// <summary>
    /// Kolejkuje kasety wraz z ich zawartością. Dla reklam pozwala to pokazać
    /// napisy (treść) zsynchronizowane z dźwiękiem odczytu w trakcie emisji.
    /// Puste sloty (null) są pomijane.
    /// </summary>
    public void EnqueuePlayables(PlayableContent[] contents)
    {
        if (contents == null)
            return;

        foreach (PlayableContent content in contents)
        {
            if (content == null)
            {
                audioQueue.Enqueue(new QueuedClip(null, null));
                continue;
            }
            audioQueue.Enqueue(new QueuedClip(content.audio, content));
        }
    }


    public static bool IsReversePitch(AudioSource source)
    {
        return source.pitch < 0f;
    }

    public static float GetClipRemainingTime(AudioSource source)
    {
        // Calculate the remainingTime of the given AudioSource,
        // if we keep playing with the same pitch.
        float remainingTime = (source.clip.length - source.time) / source.pitch;
        return IsReversePitch(source) ?
            (source.clip.length + remainingTime) :
            remainingTime;
    }

    public bool isClipPlaying(AudioClip clip)
    {
        if (clip == null || audioSource == null || audioSource.clip == null)
            return false;

        if(audioSource.clip!=clip)
            return false;

        var waitForClipRemainingTime = GetClipRemainingTime(audioSource);

        if (waitForClipRemainingTime <= 0.0f)
            return false;
        return true;
    }

    private IEnumerator ProcessAudioQueue()
    {
        skippedClip = false;
        while (audioQueue.Count > 0)
        {
            QueuedClip queued = audioQueue.Dequeue();

            audioSource.clip = queued.clip;
            if (queued.clip != null)
            {
                NowPlayingBar.SetActive(false);
                if (queued.content.GetType() == CassetteTypes.Music)
                {
                    NowPlayingBar.SetActive(true);
                    Transform songTitle = NowPlayingBar.transform.Find("Text");
                    TMP_Text text = songTitle.GetComponent<TMP_Text>();
                    text.text = $"TERAZ GRAMY: {queued.content.GetAuthor()} - {queued.content.GetName()}";
                    songTitle.GetComponent<MarqueeText>().ResetPosition();
                }
                if (queued.content != null && queued.content.GetType() == CassetteTypes.Music)
                    audioSource.volume = musicVolume;
                else
                    audioSource.volume = 1.0f;

                audioSource.Play();

                HandleAdSubtitles(queued);
            }
            // Czekamy aż skończy się audio ORAZ napisy reklamy (treść bywa dłuższa niż
            // krótki placeholderowy dźwięk — nie wolno jej uciąć).
            yield return new WaitWhile(() => (isClipPlaying(queued.clip) || AdSubtitlesActive()) && !skippedClip);
            HideAdSubtitles();
            skippedClip = false;
            yield return new WaitForSeconds(0.5f);
        }
        NowPlayingBar.SetActive(false);
        isPlayingQueue = false;

        onSequenceFinished?.Invoke();
        onSequenceFinished = null;

        timeHandler.NextHour();
    }

    /// <summary>
    /// Jeśli aktualnie odtwarzany element to reklama z treścią — pokaż napisy
    /// zsynchronizowane z długością dźwięku odczytu.
    /// </summary>
    private void HandleAdSubtitles(QueuedClip queued)
    {
        if (queued.content == null)
            return;
        if (queued.content.GetType() != CassetteTypes.Ad)
            return;

        Ad ad = queued.content as Ad;
        if (ad == null)
            return;

        string text = ad.GetContent();
        if (string.IsNullOrWhiteSpace(text))
            return;

        float duration = queued.clip != null ? queued.clip.length : 0f;
        AdSubtitleDisplay.GetOrCreate().ShowAd(text, duration);
    }

    private void HideAdSubtitles()
    {
        if (AdSubtitleDisplay.Instance != null)
            AdSubtitleDisplay.Instance.Hide();
    }

    /// <summary>Czy trwa jeszcze wyświetlanie napisów reklamy.</summary>
    private bool AdSubtitlesActive()
    {
        return AdSubtitleDisplay.Instance != null && AdSubtitleDisplay.Instance.IsShowing;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NowPlayingBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
