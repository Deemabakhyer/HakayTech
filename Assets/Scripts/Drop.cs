using UnityEngine;

public class Drop : MonoBehaviour, DropArea
{
    public void OnDrop(Drag drag)
    {
        // When a block is dropped here, make it a child of the SolutionSheet
        if (Drag.solutionSheet != null)
        {
            drag.transform.SetParent(Drag.solutionSheet);
            Debug.Log("Block added to SolutionSheet");

            // Enable the Arabic input field only when the block is correctly placed in the workspace
            // We check the 'drag' object (the block itself) for the logic script
            if (drag.TryGetComponent(out LoopBlockLogic loopLogic))
            {
                loopLogic.EnableInput();
            }
        }
        else
        {
            Debug.LogError("SolutionSheet reference is missing!");
        }
    }
}