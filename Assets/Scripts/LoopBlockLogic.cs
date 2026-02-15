using UnityEngine;
using TMPro; // Required library for handling TextMeshPro components

public class LoopBlockLogic : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The input field where the child enters the number of repetitions")]
    public TMP_InputField iterationInput;

    [Header("Settings")]
    [Tooltip("The correct number of loops required for the story (e.g., 7 for Makkah story)")]
    public int correctNumber = 7;

    /// <summary>
    /// Checks if the child's input matches the required number of repetitions.
    /// This is used by the AI Companion to provide feedback.
    /// </summary>
    /// <returns>True if input matches correctNumber, otherwise false.</returns>
    public bool IsInputCorrect()
    {
        // Check if the input field is not empty to avoid errors
        if (iterationInput == null || string.IsNullOrEmpty(iterationInput.text))
        {
            Debug.LogWarning("Input Field is empty or not assigned!");
            return false;
        }

        // Compare the text input with the correct number converted to string
        return iterationInput.text == correctNumber.ToString();
    }
}