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
        if (iterationInput == null || string.IsNullOrEmpty(iterationInput.text))
        {
            Debug.LogWarning("Input Field is empty or not assigned!");
            return false;
        }

        return iterationInput.text.Trim() == correctNumber.ToString();
    }

    // --- الدالة الناقصة الأولى: جلب القيمة النصية المدخلة من الطفل ---
    public string GetIterationValue()
    {
        return iterationInput != null ? iterationInput.text.Trim() : "";
    }

    // --- الدالة الناقصة الثانية: جلب أسماء البلوكات مرتبة برمجياً من الأعلى للأسفل بناءً على لوجيك غلا الصادي ---
    public List<string> GetSortedNestedBlockNames()
    {
        List<Transform> sortedTransforms = new List<Transform>();
        List<string> cleanNames = new List<string>();

        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            string nameLower = child.name.ToLower();
            if (child != this.transform && nameLower.Contains("block") &&
                (nameLower.Contains("(clone)") || nameLower.Contains("_copy") || nameLower.Contains("copy")))
            {
                sortedTransforms.Add(child);
            }
        }

        // استخدام نفس لوجيك غلا في الترتيب الصادي العمودي لضمان التطابق
        sortedTransforms.Sort((a, b) => b.position.y.CompareTo(a.position.y));

        foreach (Transform t in sortedTransforms)
        {
            string cleanName = t.name.Replace("_Copy", "").Replace("(Clone)", "").Trim();
            cleanNames.Add(cleanName);
        }

        return cleanNames;
    }

    /// <summary>
    /// Validates the logical sequence of blocks placed inside the Loop.
    /// Required Order for Makkah: 
    /// 1. Block2 (Tawaf Action)
    /// 2. Block3 (Dhikr/Supplication)
    /// </summary>
    public bool IsSequenceCorrect()
    {
        if (!IsInputCorrect()) return false;

        List<Transform> sortedBlocks = new List<Transform>();

        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            string nameLower = child.name.ToLower();

            if (child != this.transform && nameLower.Contains("block") &&
                (nameLower.Contains("(clone)") || nameLower.Contains("_copy") || nameLower.Contains("copy")))
            {
                sortedBlocks.Add(child);
            }
        }

        sortedBlocks.Sort((a, b) => b.position.y.CompareTo(a.position.y));

        if (sortedBlocks.Count >= 2)
        {
            bool isOrderCorrect = sortedBlocks[0].name.ToLower().Contains("block2") &&
                                 sortedBlocks[1].name.ToLower().Contains("block3");

            return isOrderCorrect;
        }

        return false;
    }
}