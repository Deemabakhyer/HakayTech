using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Drop: Handles the logic when a draggable block is dropped into the area, 
/// parenting it to the solution sheet and initializing specialized block features.
/// </summary>
public class Drop : MonoBehaviour, DropArea
{
    public void OnDrop(Drag drag)
    {
        if (Drag.solutionSheet != null)
        {
            drag.transform.SetParent(Drag.solutionSheet);
            Debug.Log("Block added to SolutionSheet");

            if (drag.TryGetComponent(out LoopBlockLogic loopLogic))
            {
                loopLogic.EnableInput();
            }

            VariableManager manager = FindObjectOfType<VariableManager>();
            if (manager != null && drag.TryGetComponent(out VariableBlock varBlock))
            {
                varBlock.PopulateDropdown(manager.GetSavedVariables());
                manager.RegisterPlacedBlock(varBlock);
            }
        }
        else
        {
            Debug.LogError("SolutionSheet reference is missing!");
        }
    }
}