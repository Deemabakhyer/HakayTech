using UnityEngine;

public class ResetButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        ResetSolution();
    }

    private void ResetSolution()
    {
        if (Drag.solutionSheet == null)
        {
            Debug.LogWarning("No SolutionSheet found");
            return;
        }

        // Delete all blocks in the solution sheet
        for (int i = Drag.solutionSheet.childCount - 1; i >= 0; i--)
        {
            Destroy(Drag.solutionSheet.GetChild(i).gameObject);
        }

        Debug.Log("Solution reset");
    }
}
