using UnityEngine;
using System.Collections.Generic;

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
            // If the block is a variable, add it to the list
            // This code only works if there is a VariableManager in the Scene
            VariableManager manager = FindObjectOfType<VariableManager>();
            if (manager != null && drag.TryGetComponent(out VariableBlock varBlock))
            {
                varBlock.PopulateDropdown(manager.GetSavedVariables());
                manager.RegisterPlacedBlock(varBlock);
            }
            else
            {
                Debug.LogError("SolutionSheet reference is missing!");
            }
        }
    }
}