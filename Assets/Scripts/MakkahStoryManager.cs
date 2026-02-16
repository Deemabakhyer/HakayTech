using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the Makkah story logic, handling the execution of the Tawaf animation
/// based on the child's programming block sequence and input.
/// </summary>
public class MakkahStoryManager : MonoBehaviour
{
    [Header("Block References")]
    [Tooltip("Reference to the loop block template")]
    public LoopBlockLogic loopBlock;

    [Header("Character Settings")]
    [Tooltip("The character GameObject that will perform the Tawaf")]
    public GameObject character;

    [Header("Path Animation")]
    [Tooltip("List of empty points defining the position, scale, and sprite for each step")]
    public List<Transform> tawafPathPoints;

    private SpriteRenderer characterRenderer;

    void Start()
    {
        // Initialize the sprite renderer reference for performance
        if (character != null)
            characterRenderer = character.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Triggered when the user clicks the "Submit" (Itmam) button.
    /// Validates the code in the Solution Sheet before starting animation.
    /// </summary>
    public void OnItmamClick()
    {
        // Dynamically find the loop block currently placed in the solution sheet
        LoopBlockLogic activeLoop = Drag.solutionSheet.GetComponentInChildren<LoopBlockLogic>();

        if (activeLoop != null)
        {
            // Case 1: Sequence and number are both correct
            if (activeLoop.IsSequenceCorrect())
            {
                Debug.Log("Logic confirmed. Starting Tawaf animation...");
                StartCoroutine(PerformTawaf());
            }
            // Case 2: The loop number is wrong (not 7)
            else if (!activeLoop.IsInputCorrect())
            {
                Debug.Log("Invalid loop count. Expected: 7.");
                // Note: AI Companion audio hint should be triggered here
            }
            // Case 3: The sequence is wrong (e.g., Block3 before Block2)
            else
            {
                Debug.Log("Invalid sequence. Ensure 'Tawaf' comes before 'Dhikr'.");
                // Note: Provide specific feedback for sequencing error
            }
        }
    }

    /// <summary>
    /// Coroutine that executes the 7-round Tawaf animation.
    /// Updates position, scale, and sprite at each path point.
    /// </summary>
    IEnumerator PerformTawaf()
    {
        // Repeat the entire path 7 times (The Islamic requirement for Tawaf)
        for (int i = 0; i < 7; i++)
        {
            foreach (Transform point in tawafPathPoints)
            {
                // Update character's Transform and Visuals to match the reference point
                character.transform.position = point.position;
                character.transform.localScale = point.localScale;

                // Match the sprite to the specific frame assigned to this point
                if (characterRenderer != null && point.GetComponent<SpriteRenderer>() != null)
                {
                    characterRenderer.sprite = point.GetComponent<SpriteRenderer>().sprite;
                }

                // Wait for a short duration to create smooth movement
                yield return new WaitForSeconds(0.05f);
            }
        }
        Debug.Log("Tawaf completed successfully.");
    }
}