using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioQueueManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
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
    private  TimeHandler timeHandler;
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    private bool isPlayingQueue = false;

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
        {
            return;
        }

        if(audioQueue.Count == 0 )
        {
            audioSource.Stop();
            HideAdSubtitles();
        }

        if( audioQueue.Count > 0 )
        {
            QueuedClip next = audioQueue.Dequeue();

            audioSource.clip = next.clip;
            audioSource.Play();
            HideAdSubtitles();
            HandleAdSubtitles(next);
        }
    }

    public void PlayClipsSequence()
    {
        if (!isPlayingQueue)
        {
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

    private IEnumerator ProcessAudioQueue()
    {
        isPlayingQueue = true;

        while (audioQueue.Count > 0)
        {
            QueuedClip queued = audioQueue.Dequeue();

            audioSource.clip = queued.clip;
            audioSource.Play();

            HandleAdSubtitles(queued);

            // Czekamy aż skończy się audio ORAZ napisy reklamy (treść bywa dłuższa niż
            // krótki placeholderowy dźwięk — nie wolno jej uciąć).
            yield return new WaitWhile(() => audioSource.isPlaying || AdSubtitlesActive());
            HideAdSubtitles();
            yield return new WaitForSeconds(0.5f);
        }

        isPlayingQueue = false;
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

    }

    // Update is called once per frame
    void Update()
    {

    }
}
