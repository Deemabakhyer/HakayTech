using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SolutionManager : MonoBehaviour
{
    // This list will now be wiped and refilled automatically every time you click Submit
    public List<CodingBlock> activeBlocks = new List<CodingBlock>();
    public Animator boyAnimator;
    public float delayBetweenActions = 2.0f;

    public void OnSubmitButtonPressed()
    {
        // 1. Force a complete refresh of the block list immediately
        RefreshBlockList();

        // 2. Stop any previous sequences to prevent overlapping animations
        StopAllCoroutines();

        // 3. Start the sequence with the freshly found blocks
        StartCoroutine(RunBlockSequence());
    }

    public void RefreshBlockList()
    {
        // This is the "Clean Slate" part: Wipe the old list completely
        activeBlocks.Clear();

        // Find only the blocks that are currently active in your Hierarchy
        CodingBlock[] foundBlocks = Object.FindObjectsByType<CodingBlock>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        activeBlocks = foundBlocks.ToList();

        Debug.Log($"<color=green>Clean Sync:</color> Found {activeBlocks.Count} active blocks in the scene.");
    }

    IEnumerator RunBlockSequence()
    {
        Debug.Log("Starting Sequence Check...");

        foreach (var block in activeBlocks)
        {
            // Only fire the animation if the block is dropped on the SolutionSheet
            if (block.IsInSolution())
            {
                Debug.Log("<color=orange>Action:</color> " + block.blockAction);
                ExecuteAction(block.blockAction);

                // Wait for the animation to finish before moving to the next block
                yield return new WaitForSeconds(delayBetweenActions);
            }
            else
            {
                Debug.Log("<color=red>Skipped:</color> " + block.name + " is not on the sheet.");
            }
        }
        Debug.Log("Sequence Finished.");
    }

    void ExecuteAction(string action)
    {
        if (boyAnimator == null) return;

        // Triggers must match the parameters in your Animator window exactly
        switch (action)
        {
            case "BoilWater":
                boyAnimator.SetTrigger("MovePotToFire");
                break;
            case "AddCoffee":
                boyAnimator.SetTrigger("AddCoffee");
                break;
            case "AddCardamom":
                boyAnimator.SetTrigger("AddCardamom");
                break;
            case "Pour":
                boyAnimator.SetTrigger("Pour");
                break;
        }
    }
}