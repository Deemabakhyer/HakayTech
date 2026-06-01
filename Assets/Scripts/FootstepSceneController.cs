using System.Collections;
using UnityEngine;

public class FootstepSceneController : MonoBehaviour
{
    [Header("Child Footsteps")]
    public AudioSource childFootstepAudio;
    public AudioClip childFootstepClip;
    public string[] childWalkingScenes;
    public string[] childStopScenes;

    [Header("Guest Footsteps")]
    public AudioSource guestFootstepAudio;
    public AudioClip guestFootstepClip;
    public string[] guestWalkingScenes;
    public string[] guestStopScenes;

    [Header("Background")]
    public AudioSource backgroundAudio;
    public float normalBackgroundVolume = 0.10f;
    public float loweredBackgroundVolume = 0.03f;

    [Header("Footstep Settings")]
    public float childVolume = 1.6f;
    public float guestVolume = 1.8f;
    public float stepInterval = 0.35f;

    private Coroutine childRoutine;
    private Coroutine guestRoutine;

    public void OnSceneChanged(GameObject currentScene)
    {
        if (currentScene == null) return;

        string sceneName = currentScene.name;

        bool childWalking = IsSceneInList(sceneName, childWalkingScenes);
        bool childStop = IsSceneInList(sceneName, childStopScenes);

        bool guestWalking = IsSceneInList(sceneName, guestWalkingScenes);
        bool guestStop = IsSceneInList(sceneName, guestStopScenes);

        if (childStop) StopChildSteps();
        else if (childWalking) StartChildSteps();
        else StopChildSteps();

        if (guestStop) StopGuestSteps();
        else if (guestWalking) StartGuestSteps();
        else StopGuestSteps();

        bool anyWalking = childRoutine != null || guestRoutine != null;

        if (backgroundAudio != null)
            backgroundAudio.volume = anyWalking ? loweredBackgroundVolume : normalBackgroundVolume;
    }

    private void StartChildSteps()
    {
        if (childRoutine == null)
            childRoutine = StartCoroutine(FootstepLoop(childFootstepAudio, childFootstepClip, childVolume));
    }

    private void StopChildSteps()
    {
        if (childRoutine != null)
        {
            StopCoroutine(childRoutine);
            childRoutine = null;
        }

        if (childFootstepAudio != null)
            childFootstepAudio.Stop();
    }

    private void StartGuestSteps()
    {
        if (guestRoutine == null)
            guestRoutine = StartCoroutine(FootstepLoop(guestFootstepAudio, guestFootstepClip, guestVolume));
    }

    private void StopGuestSteps()
    {
        if (guestRoutine != null)
        {
            StopCoroutine(guestRoutine);
            guestRoutine = null;
        }

        if (guestFootstepAudio != null)
            guestFootstepAudio.Stop();
    }

    private IEnumerator FootstepLoop(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null) yield break;

        source.loop = false;
        source.spatialBlend = 0f;

        while (true)
        {
            source.PlayOneShot(clip, volume);
            yield return new WaitForSeconds(stepInterval);
        }
    }

    private bool IsSceneInList(string sceneName, string[] sceneList)
    {
        foreach (string scene in sceneList)
        {
            if (sceneName == scene)
                return true;
        }

        return false;
    }
}