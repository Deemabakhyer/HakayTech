using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the validation logic for Loop blocks, including input validation
/// and checking the sequence of nested child blocks.
/// </summary>
public class LoopBlockLogic : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The input field for entering the number of repetitions")]
    public TMP_InputField iterationInput;

    [Header("Validation Settings")]
    [Tooltip("The expected correct number for this story's loop (e.g., 7 for Makkah)")]
    public int correctNumber = 7;

    /// <summary>
    /// Enables the input field for user interaction. 
    /// Triggered once the block is correctly placed in the workspace.
    /// </summary>
    public void EnableInput()
    {
        if (iterationInput != null)
        {
            iterationInput.interactable = true;
        }
    }

    /// <summary>
    /// Checks if the user's input matches the required repetition count.
    /// </summary>
    /// <returns>True if input matches correctNumber, false otherwise.</returns>
    public bool IsInputCorrect()
    {
        // Prevent errors if the field is missing or empty
        if (iterationInput == null || string.IsNullOrEmpty(iterationInput.text))
        {
            Debug.LogWarning("Input Field is empty or not assigned!");
            return false;
        }

        // Trim whitespace to ensure clean comparison
        return iterationInput.text.Trim() == correctNumber.ToString();
    }

    /// <summary>
    /// Validates the logical sequence of blocks placed inside the Loop.
    /// Required Order for Makkah: 
    /// 1. Block2 (Tawaf Action)
    /// 2. Block3 (Dhikr/Supplication)
    /// </summary>
    public bool IsSequenceCorrect()
    {
        // Step 1: Validate the iteration count first
        if (!IsInputCorrect()) return false;

        List<Transform> sortedBlocks = new List<Transform>();

        // Step 2: Retrieve all nested transforms to find child blocks
        // This handles cases where blocks are nested within each other (Parent-Child chain)
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            string nameLower = child.name.ToLower();

            // Filter to only include active block clones, excluding snap points or the loop itself
            if (child != this.transform && nameLower.Contains("block") && nameLower.Contains("(clone)"))
            {
                sortedBlocks.Add(child);
            }
        }

        // Step 3: Sort blocks based on their vertical (Y) world position
        // This ensures validation follows what the child sees visually (Top to Bottom)
        sortedBlocks.Sort((a, b) => b.position.y.CompareTo(a.position.y));

        // Step 4: Verify the sorted sequence matches the educational requirement
        if (sortedBlocks.Count >= 2)
        {
            // Block2 (Movement) must be at index 0, Block3 (Speech) at index 1
            bool isOrderCorrect = sortedBlocks[0].name.ToLower().Contains("block2") &&
                                 sortedBlocks[1].name.ToLower().Contains("block3");

            return isOrderCorrect;
        }

        return false;
    }
}