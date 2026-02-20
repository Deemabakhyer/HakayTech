using UnityEngine;
using System.Collections.Generic;

public class SolutionManager : MonoBehaviour
{
    // Drag your blocks from the Hierarchy into this list in the Inspector
    public List<CodingBlock> activeBlocks;
    public Animator boyAnimator;

    public void OnSubmitButtonPressed()
    {
        Debug.Log("Button was pressed!"); // This will show up in the Console at the bottom
        foreach (var block in activeBlocks)
        {
            if (block.IsInSolution())
            {
                ExecuteAction(block.blockAction);
            }
        }
    }

    void ExecuteAction(string action)
    {
        switch (action)
        {
            case "BoilWater":
                boyAnimator.SetTrigger("MovePotToFire");
                break;
            case "AddCoffee":
                // Trigger add coffee animation
                break;
        }
    }
}