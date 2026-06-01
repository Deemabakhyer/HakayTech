using System.Collections;
using UnityEngine;

public class StorySceneAudioController : MonoBehaviour
{
    [Header("Background")]
    public AudioSource backgroundAudio;
    public float normalBackgroundVolume = 0.10f;
    public float loweredBackgroundVolume = 0.03f;

    [Header("AI Arrival Layered Audio")]
    public AudioSource layeredAudioSource;
    public AudioClip[] arrivalClips;
    public float[] clipDelays;
    public float arrivalVolume = 1.0f;

    [Header("Scene Range")]
    public int startSceneIndex = 28;
    public int stopSceneIndex = 31;

    private bool hasPlayed = false;
    private Coroutine arrivalRoutine;

    public void OnSceneChanged(int sceneIndex)
    {
        if (sceneIndex == startSceneIndex && !hasPlayed)
        {
            hasPlayed = true;

            if (backgroundAudio != null)
                backgroundAudio.volume = loweredBackgroundVolume;

            if (arrivalRoutine != null)
                StopCoroutine(arrivalRoutine);

            arrivalRoutine = StartCoroutine(PlayLayeredArrival());
        }

        if (sceneIndex >= stopSceneIndex)
        {
            if (backgroundAudio != null)
                backgroundAudio.volume = normalBackgroundVolume;
        }
    }

    private IEnumerator PlayLayeredArrival()
    {
        if (layeredAudioSource == null)
            yield break;

        layeredAudioSource.volume = arrivalVolume;

        for (int i = 0; i < arrivalClips.Length; i++)
        {
            float delay = 0f;

            if (clipDelays != null && i < clipDelays.Length)
                delay = clipDelays[i];

            yield return new WaitForSeconds(delay);

            if (arrivalClips[i] != null)
                layeredAudioSource.PlayOneShot(arrivalClips[i], arrivalVolume);
        }
    }
}