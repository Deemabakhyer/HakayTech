using System.Collections;
using UnityEngine;

public class SceneSoundOnEnable : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.6f;
    public float fadeOutDuration = 0.5f;

    private Coroutine fadeRoutine;

    private void OnEnable()
    {
        if (audioSource == null)
            return;

        audioSource.volume = 0f;
        audioSource.Play();

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeIn());
    }

    private void OnDisable()
    {
        if (audioSource == null)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume =
                Mathf.Lerp(0f, 1f, timer / fadeInDuration);

            yield return null;
        }

        audioSource.volume = 1f;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume =
                Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);

            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}