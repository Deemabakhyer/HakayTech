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
        }
        else
        {
            Debug.LogError("SolutionSheet reference is missing!");
        }
    }
}