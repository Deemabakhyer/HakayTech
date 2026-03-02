using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// MakkahStoryManager: Handles the logic for the Makkah story scene, 
/// including Tawaf animation sequence, UI transitions, and character state management.
/// </summary>
public class MakkahStoryManager : MonoBehaviour
{
    [Header("Block & Logic References")]
    [Tooltip("The main programming block used by the child.")]
    public LoopBlockLogic loopBlock;

    [Header("Character Settings")]
    [Tooltip("The character that will move during Tawaf.")]
    public GameObject character;
    private SpriteRenderer characterRenderer;

    [Header("Path Animation Settings")]
    [Tooltip("Define path steps using empty Transforms (Position, Scale, Sprite).")]
    public List<Transform> tawafPathPoints;
    [Range(0.01f, 1f)]
    public float movementDelay = 0.15f;

    [Header("UI & Visual Feedback")]
    [Tooltip("The sprite that slides down to show the lap counter.")]
    public GameObject hangingBlockSprite;
    public TextMeshProUGUI lapCounterText;
    [Tooltip("The object that appears once the task is successfully completed.")]
    public GameObject successObject;
    [Tooltip("A fixed anchor point to spawn the success object.")]
    public Transform successSpawnPoint;

    [Header("Animation Curves")]
    public float slideDuration = 0.6f;
    public Vector3 hiddenPositionOffset = new Vector3(0, 5, 0);
    private Vector3 startPosition;

    [Header("Audio Settings")]
    [Tooltip("The audio source that plays Makkah ambiance or Talbiyah.")]
    public AudioSource tawafAudioSource;
    [Tooltip("Should the audio fade out smoothly at the end?")]
    public bool fadeAudioAtEnd = true;

    void Start()
    {
        // Cache references and set initial states
        if (character != null)
            characterRenderer = character.GetComponent<SpriteRenderer>();

        if (hangingBlockSprite != null)
        {
            startPosition = hangingBlockSprite.transform.position;
            // Hide the hanging block above the screen at start
            hangingBlockSprite.transform.position = startPosition + hiddenPositionOffset;
        }

        if (successObject != null)
            successObject.SetActive(false);
    }

    /// <summary>
    /// Called when the 'Itmam' (Submit) button is clicked.
    /// Validates the player's code before starting the animation.
    /// </summary>
    public void OnItmamClick()
    {
        // Locate the active loop block in the solution sheet
        LoopBlockLogic activeLoop = Drag.solutionSheet.GetComponentInChildren<LoopBlockLogic>();

        if (activeLoop != null)
        {
            // CHECK 1: Correct Sequence (Logic)
            if (activeLoop.IsSequenceCorrect())
            {
                Debug.Log("Logic Validated: Starting Tawaf.");

                // Cleanup: Remove the standing/idle model before starting movement
                Transform standingChild = character.transform.Find("StandingModel");
                if (standingChild != null)
                {
                    Destroy(standingChild.gameObject);
                }

                // Execute UI Slide and Tawaf Movement
                StopAllCoroutines();
                StartCoroutine(SlideBlock(true)); // Slide UI Down
                StartCoroutine(PerformTawaf());   // Start Character Movement
            }
            // CHECK 2: Wrong Loop Count
            else if (!activeLoop.IsInputCorrect())
            {
                Debug.LogWarning("Execution Failed: Incorrect Loop Number.");
            }
            // CHECK 3: Wrong Block Order
            else
            {
                Debug.LogWarning("Execution Failed: Invalid Sequence Order.");
            }
        }
    }

    /// <summary>
    /// Animates the hanging UI block sliding in or out of the scene.
    /// </summary>
    IEnumerator SlideBlock(bool show)
    {
        float elapsed = 0;
        Vector3 targetPos = show ? startPosition : startPosition + hiddenPositionOffset;
        Vector3 currentPos = hangingBlockSprite.transform.position;

        while (elapsed < slideDuration)
        {
            hangingBlockSprite.transform.position = Vector3.Lerp(currentPos, targetPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        hangingBlockSprite.transform.position = targetPos;
    }

    /// <summary>
    /// Core Coroutine: Cycles through 7 laps, updating character position and sprites.
    /// Ends with a cleanup and activation of the success feedback.
    /// </summary>
    IEnumerator PerformTawaf()
    {
        for (int i = 0; i < 7; i++)
        {
            if (tawafAudioSource != null)
            {
                tawafAudioSource.Play();
                tawafAudioSource.loop = true; // نضمن أنه يستمر طوال الأشواط
            }
            // Update Lap Counter UI
            if (lapCounterText != null)
                lapCounterText.text = "الشوط " + (i + 1);

            // Iterate through each point on the Tawaf path
            foreach (Transform point in tawafPathPoints)
            {
                character.transform.position = point.position;
                character.transform.localScale = point.localScale;

                // Sync the character's sprite with the path point's sprite
                SpriteRenderer pointRenderer = point.GetComponent<SpriteRenderer>();
                if (characterRenderer != null && pointRenderer != null)
                {
                    characterRenderer.sprite = pointRenderer.sprite;
                }

                yield return new WaitForSeconds(movementDelay);
            }
        }

        // --- 2. إيقاف الصوت عند انتهاء الأشواط السبعة ---
        if (tawafAudioSource != null)
        {
            if (fadeAudioAtEnd)
                StartCoroutine(FadeOutAudio(1.5f)); // يتلاشى خلال ثانية ونصف
            else
                tawafAudioSource.Stop();
        }

        // --- Sequence Completion ---

        // 1. Hide the Lap Counter UI smoothly
        if (hangingBlockSprite != null)
        {
            yield return StartCoroutine(SlideBlock(false));
        }

        // 2. Disable moving character and show success feedback
        if (successObject != null)
        {
            character.SetActive(false);

            // Position the success object at the predefined fixed point
            if (successSpawnPoint != null)
                successObject.transform.position = successSpawnPoint.position;
            else
                successObject.transform.position = new Vector3(1.5f, -2.0f, 0f);

            successObject.SetActive(true);

            if (lapCounterText != null)
                lapCounterText.text = "Done";

            Debug.Log("Story Completed: Success object activated.");
        }
    }


    /// <summary>
    /// Smoothly lowers the volume before stopping the audio.
    /// </summary>
    IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = tawafAudioSource.volume;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tawafAudioSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
            yield return null;
        }

        tawafAudioSource.Stop();
        tawafAudioSource.volume = startVolume; // إعادة القوة للأصل للمرة القادمة
    }
}