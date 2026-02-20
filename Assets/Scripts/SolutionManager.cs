using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SolutionManager : MonoBehaviour
{
    public List<CodingBlock> activeBlocks = new List<CodingBlock>();
    public Animator boyAnimator;

    void Awake()
    {
        RefreshBlockList();
    }

    // This function can be called manually or automatically
    public void RefreshBlockList()
    {
        activeBlocks.Clear();

        // This search is more thorough for nested objects
        CodingBlock[] foundBlocks = FindObjectsByType<CodingBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (foundBlocks.Length > 0)
        {
            activeBlocks.AddRange(foundBlocks);
            Debug.Log($"<color=green>Success!</color> Found {activeBlocks.Count} blocks automatically.");
        }
        else
        {
            Debug.LogError("Still found 0 blocks. Are the CodingBlock scripts attached to the objects?");
        }
    }

    public void OnSubmitButtonPressed()
    {
        foreach (var block in activeBlocks)
        {
            // TEMPORARY: This ignores the drag-and-drop check 
            // just to prove the button and boy are connected.
            ExecuteAction(block.blockAction);
        }
    }

    void ExecuteAction(string action)
    {
        if (boyAnimator == null) return;

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