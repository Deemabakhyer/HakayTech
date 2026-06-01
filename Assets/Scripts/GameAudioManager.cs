using System.Collections;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [Header("Background Music")]
    public AudioSource backgroundAudio;
    public float normalVolume = 0.55f;
    public float duckVolume = 0.18f;
    public float fadeDuration = 0.4f;

    [Header("Guest Welcome Audio")]
    public AudioSource voiceAudio;
    public AudioClip guestOneWelcome;
    public AudioClip guestTwoWelcome;

    [Header("Mute Background In These Scenes")]
    public string[] mutedBackgroundScenes =
    {
        "Scene_30",
        "Scene_31",
        "Scene_32"
    };

    [Header("Welcome Scene Names")]
    public string guestOneSceneName = "Scene_16";
    public string guestTwoSceneName = "Scene_26";

    private Coroutine volumeRoutine;
    private bool guestOnePlayed = false;
    private bool guestTwoPlayed = false;

    private void Start()
    {
        if (backgroundAudio != null)
        {
            backgroundAudio.loop = true;
            backgroundAudio.volume = normalVolume;

            if (!backgroundAudio.isPlaying)
                backgroundAudio.Play();
        }
    }

    public void HandleBackgroundForScene(GameObject currentScene)
    {
        if (currentScene == null) return;

        if (ShouldMuteBackground(currentScene.name))
            FadeBackgroundTo(0f);
        else
            FadeBackgroundTo(normalVolume);
    }

    public bool IsGuestWelcomeScene(GameObject currentScene)
    {
        if (currentScene == null) return false;

        string sceneName = currentScene.name;

        if (sceneName == guestOneSceneName && !guestOnePlayed)
            return true;

        if (sceneName == guestTwoSceneName && !guestTwoPlayed)
            return true;

        return false;
    }

    public IEnumerator PlayWelcomeForScene(GameObject currentScene)
    {
        if (currentScene == null) yield break;

        AudioClip clip = null;

        if (currentScene.name == guestOneSceneName && !guestOnePlayed)
        {
            guestOnePlayed = true;
            clip = guestOneWelcome;
        }
        else if (currentScene.name == guestTwoSceneName && !guestTwoPlayed)
        {
            guestTwoPlayed = true;
            clip = guestTwoWelcome;
        }

        if (clip == null || voiceAudio == null)
            yield break;

        FadeBackgroundTo(duckVolume);

        yield return new WaitForSeconds(0.25f);

        voiceAudio.clip = clip;
        voiceAudio.Play();

        yield return new WaitForSeconds(clip.length);

        FadeBackgroundTo(normalVolume);
    }

    private bool ShouldMuteBackground(string sceneName)
    {
        foreach (string mutedScene in mutedBackgroundScenes)
        {
            if (sceneName == mutedScene)
                return true;
        }

        return false;
    }

    private void FadeBackgroundTo(float targetVolume)
    {
        if (backgroundAudio == null) return;

        if (volumeRoutine != null)
            StopCoroutine(volumeRoutine);

        volumeRoutine = StartCoroutine(FadeVolumeRoutine(targetVolume));
    }

    private IEnumerator FadeVolumeRoutine(float targetVolume)
    {
        float startVolume = backgroundAudio.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            backgroundAudio.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        backgroundAudio.volume = targetVolume;
    }
}