using System.Collections.Generic;
using UnityEngine;

public class BlockChecker : MonoBehaviour
{
    public Transform selectedBlocksPanel;
    public List<string> correctOrder;

    [Header("Controllers")]
    public AIDialogueController aiController;
    public ScreenStateVisual screenStateVisual;
    public PlayerPathController playerPathController;

    [Header("Success Celebration")]
    public SuccessCelebrationTrigger successCelebration;

    [Header("AI Feedback")]
    public StageAIFeedback aiFeedback = new StageAIFeedback { storyKey = "riyadh" };

    [Header("Block State Mapping")]
    public int startState = 5;
    public int firstMoveForwardState = 6;
    public int turnRightState = 8;
    public int stopState = 9;
    public int turnLeftState = 12;
    public int secondMoveForwardState = 13;
    public int endState = 15;

    [Header("Wrong States")]
    public int wrongState = 8;
    private bool challengeCompleted;

    public void EvaluateLiveState()
    {
        if (selectedBlocksPanel == null)
        {
            Debug.LogError("[BlockChecker] selectedBlocksPanel is NULL on object: " + name);
            return;
        }

        List<DragBlock> placedBlocks = GetPlacedBlocks();

        if (placedBlocks.Count == 0)
        {
            if (playerPathController != null)
                playerPathController.SyncWithBlocks(placedBlocks, correctOrder);

            return;
        }

        int state = GetGameplayState(placedBlocks);

        if (screenStateVisual != null)
            screenStateVisual.ShowState(state);

        if (aiController != null)
            aiController.ShowState(state);

        if (playerPathController != null)
            playerPathController.SyncWithBlocks(placedBlocks, correctOrder);
    }

    public void CheckBlocks()
    {
        if (challengeCompleted)
            return;

        if (selectedBlocksPanel == null)
        {
            Debug.LogError("[BlockChecker] selectedBlocksPanel is NULL on object: " + name);
            return;
        }

        List<DragBlock> placedBlocks = GetPlacedBlocks();

        if (placedBlocks.Count != correctOrder.Count)
        {
            Debug.Log("Not complete yet.");
            aiFeedback.RequestWrong(
                this,
                "riyadh",
                "riyadh_incomplete:" + BuildBlocksSignature(placedBlocks),
                BuildRiyadhFeedbackLine(placedBlocks)
            );

            if (screenStateVisual != null)
                screenStateVisual.ShowError();

            if (aiController != null)
                aiController.ShowErrorMessage();

            return;
        }

        int wrongIndex = GetFirstWrongIndex(placedBlocks);

        if (wrongIndex == -1)
        {
            Debug.Log("SUCCESS: Correct order!");
            challengeCompleted = true;

            if (screenStateVisual != null)
                screenStateVisual.ShowCompletedPath();

            if (aiController != null)
                aiController.ShowSuccessMessage();

            if (successCelebration != null)
                successCelebration.PlaySuccess();
            else
                Debug.LogWarning("Success Celebration is not assigned in BlockChecker.");

            aiFeedback.RequestSuccess(
                this,
                "riyadh",
                "riyadh_success",
                "أحسنت، صححت الاتجاه واختبرت الحل بنجاح."
            );
            GameEvents.OnChallengeComplete?.Invoke();
        }
        else
        {
            Debug.Log("WRONG ORDER");
            aiFeedback.RequestWrong(
                this,
                "riyadh",
                "riyadh_wrong:" + BuildBlocksSignature(placedBlocks),
                BuildRiyadhFeedbackLine(placedBlocks)
            );

            if (screenStateVisual != null)
                screenStateVisual.ShowError();

            if (aiController != null)
                aiController.ShowErrorMessage();
        }
    }

    private int GetGameplayState(List<DragBlock> placedBlocks)
    {
        int wrongIndex = GetFirstWrongIndex(placedBlocks);

        if (wrongIndex != -1)
            return wrongState;

        DragBlock lastBlock = placedBlocks[placedBlocks.Count - 1];
        string id = lastBlock.blockID.Trim();

        int moveForwardCount = CountBlock(placedBlocks, "MoveForward");

        switch (id)
        {
            case "Start":
                return startState;

            case "MoveForward":
                return moveForwardCount == 1 ? firstMoveForwardState : secondMoveForwardState;

            case "TurnRight":
                return turnRightState;

            case "Stop":
                return stopState;

            case "TurnLeft":
                return turnLeftState;

            case "End":
                return endState;

            default:
                return startState;
        }
    }

    private int CountBlock(List<DragBlock> placedBlocks, string blockID)
    {
        int count = 0;

        for (int i = 0; i < placedBlocks.Count; i++)
        {
            if (placedBlocks[i].blockID.Trim() == blockID)
                count++;
        }

        return count;
    }

    private int GetFirstWrongIndex(List<DragBlock> placedBlocks)
    {
        for (int i = 0; i < placedBlocks.Count; i++)
        {
            if (i >= correctOrder.Count)
                return i;

            string playerID = placedBlocks[i].blockID.Trim();
            string correctID = correctOrder[i].Trim();

            if (playerID != correctID)
                return i;
        }

        return -1;
    }

    private string BuildRiyadhFeedbackLine(List<DragBlock> placedBlocks)
    {
        if (placedBlocks.Count == 0)
            return "ضع أول خطوة لتصحيح مسار الشخصية.";

        int wrongIndex = GetFirstWrongIndex(placedBlocks);
        if (wrongIndex >= 0 && wrongIndex < correctOrder.Count)
            return "الخطوة " + GetArabicStepLabel(wrongIndex) + " غير مناسبة لمسار التصحيح.";

        if (placedBlocks.Count < correctOrder.Count)
            return "الخطوات ناقصة؛ أكمل التصحيح ثم اختبر الحل.";

        if (placedBlocks.Count > correctOrder.Count)
            return "هناك خطوة زائدة؛ أبق خطوات التصحيح المطلوبة فقط.";

        return "راجع ترتيب خطوات التصحيح قبل الاختبار.";
    }

    private string GetArabicStepLabel(int index)
    {
        switch (index)
        {
            case 0: return "الأولى";
            case 1: return "الثانية";
            case 2: return "الثالثة";
            case 3: return "الرابعة";
            case 4: return "الخامسة";
            case 5: return "السادسة";
            default: return "الحالية";
        }
    }

    private List<DragBlock> GetPlacedBlocks()
    {
        List<DragBlock> placedBlocks = new List<DragBlock>();

        for (int i = 0; i < selectedBlocksPanel.childCount; i++)
        {
            DragBlock block = selectedBlocksPanel.GetChild(i).GetComponent<DragBlock>();

            if (block != null)
                placedBlocks.Add(block);
        }

        return placedBlocks;
    }

    private string BuildBlocksSignature(List<DragBlock> placedBlocks)
    {
        List<string> blockIds = new List<string>();

        foreach (DragBlock block in placedBlocks)
            blockIds.Add(block != null ? block.blockID.Trim() : "");

        return string.Join("|", blockIds);
    }
}
