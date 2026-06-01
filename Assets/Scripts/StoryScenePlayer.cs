using System.Collections;
using UnityEngine;

public class StoryScenePlayer : MonoBehaviour
{
    [Header("Left Story Scenes In Order")]
    public GameObject[] scenes;

    [Header("Timing")]
    public float secondsPerScene = 1.2f;

    [Header("Special Scene Hold")]
    public int specialHoldSceneIndex = 28;
    public float specialHoldDuration = 3.0f;

    [Header("Board Pause Scene")]
    public int boardPauseSceneIndex = 32;
    private bool waitingForBoardClick = false;

    [Header("Success Celebration")]
    public int successSceneIndex = 35;
    public SuccessBurstPlayer successBurstPlayer;

    [Header("Success Sound")]
    public int successSoundSceneIndex = 36;
    public AudioSource successAudioSource;
    public AudioClip successClip;

    [Header("Audio Cue")]
    public StorySceneAudioController audioController;

    [Header("Game Audio")]
    public GameAudioManager gameAudioManager;

    [Header("Footsteps")]
    public FootstepSceneController footstepController;

    [Header("Optional")]
    public CompletionPopupController successPopup;

    private Coroutine storyRoutine;
    private System.Action onStoryCompleted;

    private void Start()
    {
        if (scenes.Length > 0)
            ShowOnlyScene(0);
    }

    public void PlayStory(System.Action completedCallback = null)
    {
        if (storyRoutine != null)
            StopCoroutine(storyRoutine);

        onStoryCompleted = completedCallback;
        waitingForBoardClick = false;
        storyRoutine = StartCoroutine(PlayStoryRoutine());
    }

    private IEnumerator PlayStoryRoutine()
    {
        for (int i = 0; i < scenes.Length; i++)
        {
            ShowOnlyScene(i);

            // Success Celebration
            if (i == successSceneIndex && successBurstPlayer != null)
                successBurstPlayer.PlayCelebration();

            // Success Sound
            if (i == successSoundSceneIndex)
            {
                if (successAudioSource != null && successClip != null)
                    successAudioSource.PlayOneShot(successClip);
            }

            // Scene Audio
            if (audioController != null)
                audioController.OnSceneChanged(i);

            // Background + Welcome Audio
            if (gameAudioManager != null)
            {
                gameAudioManager.HandleBackgroundForScene(scenes[i]);

                if (gameAudioManager.IsGuestWelcomeScene(scenes[i]))
                    yield return StartCoroutine(
                        gameAudioManager.PlayWelcomeForScene(scenes[i])
                    );
            }

            // Footsteps
            if (footstepController != null)
                footstepController.OnSceneChanged(scenes[i]);

            // Pause At Board
            if (i == boardPauseSceneIndex)
            {
                waitingForBoardClick = true;

                Debug.Log(
                    "Paused at make_areeka board. Waiting for click."
                );

                yield return new WaitUntil(
                    () => waitingForBoardClick == false
                );
            }

            // Special Hold
            if (i == specialHoldSceneIndex)
                yield return new WaitForSeconds(specialHoldDuration);
            else
                yield return new WaitForSeconds(secondsPerScene);
        }

        System.Action completedCallback = onStoryCompleted;
        onStoryCompleted = null;
        completedCallback?.Invoke();

        if (successPopup != null)
            successPopup.ShowPopup();
    }

    public void ContinueAfterBoardClick()
    {
        if (waitingForBoardClick)
        {
            Debug.Log(
                "make_areeka board clicked. Continuing story."
            );

            waitingForBoardClick = false;
        }
    }

    private void ShowOnlyScene(int index)
    {
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i] != null)
                scenes[i].SetActive(i == index);
        }
    }
}
