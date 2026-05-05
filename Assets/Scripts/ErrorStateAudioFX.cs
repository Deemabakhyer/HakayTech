using System.Collections;
using UnityEngine;

public class ErrorStateAudioFX : MonoBehaviour
{
    [Header("References")]
    public ScreenStateVisual screenStateVisual;

    [Header("Error Audio")]
    public AudioSource errorAudioSource;
    public AudioClip errorClip;

    [Header("Background Music Ducking")]
    public AudioSource backgroundMusic;
    public float loweredMusicVolume = 0.35f;
    public float fadeDownDuration = 0.12f;
    public float fadeUpDuration = 0.35f;

    private int lastState = -1;
    private float normalMusicVolume = 1f;

    void Start()
    {
        if (backgroundMusic != null)
            normalMusicVolume = backgroundMusic.volume;
    }

    void Update()
    {
        if (screenStateVisual == null)
            return;

        int currentState = screenStateVisual.CurrentState;

        if (currentState == lastState)
            return;

        lastState = currentState;

        if (currentState == 8)
            PlayErrorFX();
    }

    void PlayErrorFX()
    {
        StopAllCoroutines();

        if (backgroundMusic != null)
            StartCoroutine(DuckMusic());

        if (errorAudioSource != null && errorClip != null)
            errorAudioSource.PlayOneShot(errorClip);
    }

    IEnumerator DuckMusic()
    {
        yield return FadeMusic(backgroundMusic.volume, loweredMusicVolume, fadeDownDuration);

        if (errorClip != null)
            yield return new WaitForSeconds(errorClip.length);

        yield return FadeMusic(backgroundMusic.volume, normalMusicVolume, fadeUpDuration);
    }

    IEnumerator FadeMusic(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            backgroundMusic.volume = Mathf.Lerp(from, to, p);
            yield return null;
        }

        backgroundMusic.volume = to;
    }
}