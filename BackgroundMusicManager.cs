using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
#pragma warning disable UDR0001 // Domain Reload Analyzer
    public static BackgroundMusicManager Instance;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    [Header("Music Playlist")]
    public List<AudioClip> musicPlaylist = new();
    public bool loopPlaylist = true;

    [Header("Crossfade Settings")]
    public float crossfadeDuration = 1.5f;

    private AudioSource activeSource;
    private AudioSource fadeSource;

    private int currentIndex = -1;
    public AudioClip LastClipPlayed => activeSource?.clip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            activeSource = gameObject.AddComponent<AudioSource>();
            fadeSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            activeSource = sources[0];
            fadeSource = sources[1];
        }

        activeSource.loop = true;
        fadeSource.loop = true;
    }

    public void PlayNextInPlaylist()
    {
        if (musicPlaylist.Count == 0)
        {
            Debug.LogWarning("[BGM] Playlist is empty.");
            return;
        }

        currentIndex++;

        if (currentIndex >= musicPlaylist.Count)
        {
            if (loopPlaylist)
                currentIndex = 0;
            else
            {
                Debug.Log("[BGM] End of playlist reached.");
                return;
            }
        }

        CrossfadeTo(musicPlaylist[currentIndex]);
    }

    public void CrossfadeTo(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[BGM] Attempted to crossfade to null clip.");
            return;
        }

        if (activeSource.clip == clip && activeSource.isPlaying)
            return; // Already playing

        StopAllCoroutines();
        StartCoroutine(CrossfadeRoutine(clip));
    }

    private System.Collections.IEnumerator CrossfadeRoutine(AudioClip newClip)
    {
        fadeSource.clip = newClip;
        fadeSource.volume = 0f;
        fadeSource.Play();

        float time = 0f;
        float initialVolume = activeSource.volume;

        while (time < crossfadeDuration)
        {
            time += Time.deltaTime;
            float t = time / crossfadeDuration;

            fadeSource.volume = Mathf.Lerp(0f, 1f, t);
            activeSource.volume = Mathf.Lerp(initialVolume, 0f, t);
            yield return null;
        }

        var temp = activeSource;
        activeSource = fadeSource;
        fadeSource = temp;

        fadeSource.Stop();
        fadeSource.clip = null;
    }
}
