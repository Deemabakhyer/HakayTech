using System.Collections;
using UnityEngine;

public class HomeAudioController : MonoBehaviour
{
    [Header("Music")]
    public AudioSource backgroundMusic;
    public float targetVolume = 0.45f;
    public float fadeInDuration = 2f;

    [Header("Ducking")]
    public float duckVolume = 0.18f;
    public float duckDownDuration = 0.15f;
    public float duckUpDuration = 0.45f;

    private Coroutine musicRoutine;

    void Start()
    {
        FadeInMusic();
    }

    public void FadeInMusic()
    {
        if (backgroundMusic == null) return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        backgroundMusic.volume = 0f;

        if (!backgroundMusic.isPlaying)
            backgroundMusic.Play();

        musicRoutine = StartCoroutine(FadeVolume(0f, targetVolume, fadeInDuration));
    }

    public void DuckMusic(float holdDuration = 0.5f)
    {
        if (backgroundMusic == null) return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(DuckRoutine(holdDuration));
    }

    IEnumerator DuckRoutine(float holdDuration)
    {
        float currentVolume = backgroundMusic.volume;

        yield return FadeVolume(currentVolume, duckVolume, duckDownDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return FadeVolume(backgroundMusic.volume, targetVolume, duckUpDuration);
    }

    IEnumerator FadeVolume(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            backgroundMusic.volume = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, p));
            yield return null;
        }

        backgroundMusic.volume = to;
    }
}