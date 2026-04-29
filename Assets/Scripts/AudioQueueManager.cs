using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioQueueManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public  TimeHandler timeHandler;
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
        }

        if( audioQueue.Count > 0 )
        {
            AudioClip clip = audioQueue.Dequeue();

            audioSource.clip = clip;
            audioSource.Play();
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
            audioQueue.Enqueue(clip);
        }
    }

    public void EnqueueClip(AudioClip clip)
    {
        audioQueue.Enqueue(clip);
    }

    private IEnumerator ProcessAudioQueue()
    {
        isPlayingQueue = true;

        while (audioQueue.Count > 0)
        {
            AudioClip clip = audioQueue.Dequeue();

            audioSource.clip = clip;
            audioSource.Play();

            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(0.5f);
        }

        isPlayingQueue = false;
        timeHandler.NextHour();
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
