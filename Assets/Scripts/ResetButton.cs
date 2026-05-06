using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public Transform solutionSheet;
    public BlockChecker blockChecker;
    public BlockExecutionController executionController;

    public void ResetSolution()
    {
        if (solutionSheet == null)
        {
            Debug.LogWarning("No SolutionSheet assigned");
            return;
        }

        for (int i = solutionSheet.childCount - 1; i >= 0; i--)
        {
            Destroy(solutionSheet.GetChild(i).gameObject);
        }

        if (executionController != null)
            executionController.ResetPreview();

        // انتظر فريم واحد ضمنيًا بعد الحذف ثم حدّث الحالة
        Invoke(nameof(RefreshAfterReset), 0.02f);
    }

    void RefreshAfterReset()
    {
        if (blockChecker != null)
            blockChecker.EvaluateLiveState();
    }
}
