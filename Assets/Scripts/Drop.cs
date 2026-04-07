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

            // إذا كان البلوك متغير، أضفه للقائمة
            // هذا الكود يشتغل بس إذا في VariableManager في الـ Scene
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