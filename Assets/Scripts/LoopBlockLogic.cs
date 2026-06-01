using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the validation logic for Loop blocks, including input validation
/// and checking the sequence of nested child blocks.
/// </summary>
public class LoopBlockLogic : MonoBehaviour
{
    private static readonly string[] ExpectedBlockNameParts =
    {
        "block2",
        "block3"
    };

    [Header("UI References")]
    [Tooltip("The input field for entering the number of repetitions")]
    public TMP_InputField iterationInput;

    [Header("Validation Settings")]
    [Tooltip("The expected correct number for this story's loop (e.g., 7 for Makkah)")]
    public int correctNumber = 7;

    private bool inputListenerRegistered;

    private void OnDestroy()
    {
        if (iterationInput != null && inputListenerRegistered)
            iterationInput.onEndEdit.RemoveListener(OnIterationInputSubmitted);
    }

    /// <summary>
    /// Enables the input field for user interaction. 
    /// Triggered once the block is correctly placed in the workspace.
    /// </summary>
    public void EnableInput()
    {
        if (iterationInput != null)
        {
            iterationInput.interactable = true;
            RegisterInputListener();
        }
    }

    public string GetIterationValue()
    {
        return iterationInput != null ? iterationInput.text.Trim() : "";
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
        return GetIterationValue() == correctNumber.ToString();
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

        List<Transform> sortedBlocks = GetSortedNestedBlocks();

        if (sortedBlocks.Count != ExpectedBlockNameParts.Length)
            return false;

        return GetFirstWrongBlockIndex(sortedBlocks) == -1;
    }

    public List<Transform> GetSortedNestedBlocks()
    {
        List<Transform> sortedBlocks = new List<Transform>();

        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            string nameLower = child.name.ToLower();

            if (child != transform &&
                nameLower.Contains("block") &&
                (nameLower.Contains("(clone)") ||
                 nameLower.Contains("_copy") ||
                 nameLower.Contains("copy")))
            {
                sortedBlocks.Add(child);
            }
        }

        sortedBlocks.Sort((a, b) => b.position.y.CompareTo(a.position.y));
        return sortedBlocks;
    }

    public List<string> GetSortedNestedBlockNames()
    {
        List<string> names = new List<string>();

        foreach (Transform block in GetSortedNestedBlocks())
            names.Add(CleanBlockName(block.name));

        return names;
    }

    public int GetFirstWrongBlockIndex()
    {
        return GetFirstWrongBlockIndex(GetSortedNestedBlocks());
    }

    private int GetFirstWrongBlockIndex(List<Transform> sortedBlocks)
    {
        int countToCheck = Mathf.Min(sortedBlocks.Count, ExpectedBlockNameParts.Length);

        for (int i = 0; i < countToCheck; i++)
        {
            string blockName = sortedBlocks[i].name.ToLower();

            if (!blockName.Contains(ExpectedBlockNameParts[i]))
                return i;
        }

        if (sortedBlocks.Count > ExpectedBlockNameParts.Length)
            return ExpectedBlockNameParts.Length;

        return -1;
    }

    private string CleanBlockName(string blockName)
    {
        return blockName
            .Replace("_Copy", "")
            .Replace("(Clone)", "")
            .Replace("Copy", "")
            .Trim();
    }

    private void RegisterInputListener()
    {
        if (iterationInput == null || inputListenerRegistered)
            return;

        iterationInput.onEndEdit.AddListener(OnIterationInputSubmitted);
        inputListenerRegistered = true;
    }

    private void OnIterationInputSubmitted(string _)
    {
        MakkahStoryManager manager = FindObjectOfType<MakkahStoryManager>();

        if (manager != null)
            manager.OnLoopIterationEdited(this);
    }
}
