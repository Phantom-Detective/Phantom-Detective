using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public static Audio Instance;

    [Header("Audio Sources")]
    public AudioSource starterSource;
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip starterClip;
    public AudioClip bgmClip;
    public List<AudioClip> randomClips = new List<AudioClip>();

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmNormalVolume = 0.7f;
    [Range(0f, 1f)] public float bgmDuckedVolume = 0.15f;
    public float duckFadeDuration = 0.6f;
    public float bgmFadeInDuration = 1.2f;

    [Header("Random SFX Timing")]
    public float minInterval = 20f;
    public float maxInterval = 40f;

    private Coroutine bgmVolumeCoroutine;
    private Coroutine randomSfxCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (starterSource == null) starterSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(GameAudioSequence());
    }

    private IEnumerator GameAudioSequence()
    {
        // 1. Play starter sound
        if (starterClip != null)
        {
            starterSource.clip = starterClip;
            starterSource.loop = false;
            starterSource.volume = 1f;
            starterSource.Play();
            yield return new WaitForSeconds(starterClip.length);
        }

        // 2. Start background music
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0f;
            bgmSource.Play();
            yield return StartCoroutine(FadeBgmVolume(bgmNormalVolume, bgmFadeInDuration));
        }

        // 3. Start random SFX loop
        randomSfxCoroutine = StartCoroutine(RandomSfxLoop());
    }

    private IEnumerator RandomSfxLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (randomClips == null || randomClips.Count == 0) continue;

            AudioClip selectedClip = randomClips[Random.Range(0, randomClips.Count)];
            if (selectedClip == null) continue;

            // Duck BGM down
            if (bgmVolumeCoroutine != null) StopCoroutine(bgmVolumeCoroutine);
            bgmVolumeCoroutine = StartCoroutine(FadeBgmVolume(bgmDuckedVolume, duckFadeDuration));

            // Play random SFX
            sfxSource.PlayOneShot(selectedClip);
            yield return new WaitForSeconds(selectedClip.length);

            // Restore BGM up
            if (bgmVolumeCoroutine != null) StopCoroutine(bgmVolumeCoroutine);
            bgmVolumeCoroutine = StartCoroutine(FadeBgmVolume(bgmNormalVolume, duckFadeDuration));
        }
    }

    private IEnumerator FadeBgmVolume(float targetVolume, float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    public void StopAllAudio()
    {
        if (bgmVolumeCoroutine != null) StopCoroutine(bgmVolumeCoroutine);
        if (randomSfxCoroutine != null) StopCoroutine(randomSfxCoroutine);

        starterSource?.Stop();
        bgmSource?.Stop();
        sfxSource?.Stop();
    }
}