using System.Collections;
using UnityEngine;

public class PlayerSpawnIntro : MonoBehaviour
{
    [Header("References")]
    public RectTransform playerCharacter;
    public RectTransform pointStart;

    [Header("Spawn Settings")]
    public float spawnOffsetY = 250f;
    public float spawnDuration = 0.55f;
    public float startScale = 0.7f;
    public float endScale = 1f;

    [Header("Spawn Audio")]
    public AudioSource spawnAudioSource;
    public AudioClip spawnClip;

    [Header("Background Music Ducking")]
    public AudioSource backgroundMusic;
    public float normalMusicVolume = 1f;
    public float loweredMusicVolume = 0.35f;
    public float fadeDownDuration = 0.15f;
    public float fadeUpDuration = 0.35f;

    public void PlaySpawn()
    {
        Debug.Log("PlaySpawn CALLED");

        StopAllCoroutines();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter is NULL");
            yield break;
        }

        if (pointStart == null)
        {
            Debug.LogError("Point_Start is NULL");
            yield break;
        }

        if (backgroundMusic != null)
            StartCoroutine(FadeMusic(backgroundMusic.volume, loweredMusicVolume, fadeDownDuration));

        if (spawnAudioSource != null && spawnClip != null)
            spawnAudioSource.PlayOneShot(spawnClip);

        Vector3 targetWorldPos = pointStart.position;
        Vector3 startWorldPos = targetWorldPos + new Vector3(0f, spawnOffsetY, 0f);

        playerCharacter.gameObject.SetActive(true);
        playerCharacter.position = startWorldPos;
        playerCharacter.localScale = Vector3.one * startScale;

        float t = 0f;

        while (t < spawnDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / spawnDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, p);

            playerCharacter.position = Vector3.Lerp(startWorldPos, targetWorldPos, smooth);

            float scale = Mathf.Lerp(startScale, endScale, smooth);
            playerCharacter.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        playerCharacter.position = targetWorldPos;
        playerCharacter.localScale = Vector3.one * endScale;

        if (backgroundMusic != null)
            StartCoroutine(FadeMusic(backgroundMusic.volume, normalMusicVolume, fadeUpDuration));

        Debug.Log("Spawn finished");
    }

    IEnumerator FadeMusic(float from, float to, float duration)
    {
        if (backgroundMusic == null)
            yield break;

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