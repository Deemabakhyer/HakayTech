using System.Collections.Generic;
using UnityEngine;

public class RiyadhBlockExecutionController : MonoBehaviour
{
    [Header("Blocks Source")]
    public Transform selectedBlocksPanel;

    [Header("Character Inside Screen")]
    public Transform character;

    [Header("Waypoints")]
    public Transform pointStart;
    public Transform pointForward1;
    public Transform pointRightWrong;
    public Transform pointStop;
    public Transform pointForward2;
    public Transform pointEnd;
    [Header("Success UI Integration")]
    public CompletionPopupController completionPopup;

    [Header("AI Feedback")]
    public StageAIFeedback aiFeedback = new StageAIFeedback { storyKey = "riyadh" };

    private bool successTriggered;

    public void ResetPreview()
    {
        if (character != null && pointStart != null)
        {
            character.position = pointStart.position;
            character.rotation = Quaternion.identity;
        }
    }

    public void PreviewFromCurrentBlocks()
    {
        if (selectedBlocksPanel == null || character == null || pointStart == null)
            return;

        List<string> blocks = ReadBlocksFromPanel();
        PreviewFromBlocks(blocks);
    }

    public void PreviewFromBlocks(List<string> blocks)
    {
        if (character == null || pointStart == null)
            return;

        Transform targetPoint = pointStart;
        float targetRotationZ = 0f;

        bool movedOnce = false;
        bool turnedRight = false;
        bool stopped = false;
        bool fixedLeft = false;
        bool movedAfterFix = false;
        bool reachedEnd = false;

        foreach (string raw in blocks)
        {
            string block = raw.Trim();

            switch (block)
            {
                case "Start":
                    break;

                case "MoveForward":
                    if (!movedOnce)
                    {
                        movedOnce = true;
                        targetPoint = pointForward1 != null ? pointForward1 : pointStart;
                    }
                    else if (turnedRight && !fixedLeft)
                    {
                        if (!stopped)
                            targetPoint = pointRightWrong != null ? pointRightWrong : targetPoint;
                        else
                            targetPoint = pointStop != null ? pointStop : targetPoint;
                    }
                    else if (fixedLeft && !movedAfterFix)
                    {
                        movedAfterFix = true;
                        targetPoint = pointForward2 != null ? pointForward2 : targetPoint;
                    }
                    else if (movedAfterFix && !reachedEnd)
                    {
                        reachedEnd = true;
                        targetPoint = pointEnd != null ? pointEnd : targetPoint;
                    }
                    else if (!turnedRight)
                    {
                        targetPoint = pointForward2 != null ? pointForward2 : targetPoint;
                    }
                    break;

                case "TurnRight":
                    turnedRight = true;
                    targetRotationZ = -90f;

                    if (movedOnce && pointRightWrong != null)
                        targetPoint = pointRightWrong;
                    break;

                case "TurnLeft":
                    if (turnedRight)
                    {
                        fixedLeft = true;
                        targetRotationZ = 0f;
                    }
                    else
                    {
                        targetRotationZ = 90f;
                    }
                    break;

                case "Stop":
                    stopped = true;
                    if (pointStop != null)
                        targetPoint = pointStop;
                    break;

                case "End":
                    if (fixedLeft && movedAfterFix && pointEnd != null)
                    {
                        reachedEnd = true;
                        targetPoint = pointEnd;
                    }
                    break;
            }
        }

        character.position = targetPoint != null ? targetPoint.position : pointStart.position;
        character.rotation = Quaternion.Euler(0f, 0f, targetRotationZ);

        if (reachedEnd && targetPoint == pointEnd)
        {
            if (successTriggered)
                return;

            successTriggered = true;

            aiFeedback.RequestSuccess(
                this,
                "riyadh",
                "riyadh_success",
                "أحسنت، صححت الاتجاه واختبرت الحل بنجاح."
            );

            if (completionPopup != null)
            {
                completionPopup.ShowPopup();
            }
        }
    }

    private List<string> ReadBlocksFromPanel()
    {
        List<string> blocks = new List<string>();

        for (int i = 0; i < selectedBlocksPanel.childCount; i++)
        {
            DragBlock block = selectedBlocksPanel.GetChild(i).GetComponent<DragBlock>();
            if (block != null)
                blocks.Add(block.blockID.Trim());
        }

        return blocks;
    }

   
    public void TriggerRiyadhSuccessPopup()
    {
        if (successTriggered)
            return;

        successTriggered = true;

        aiFeedback.RequestSuccess(
            this,
            "riyadh",
            "riyadh_success",
            "أحسنت، صححت الاتجاه واختبرت الحل بنجاح."
        );

        if (completionPopup != null)
        {
            completionPopup.ShowPopup();
        }
        else
        {
            Debug.LogError("Riyadh Block Controller: completionPopup is missing in Inspector!");
        }
    }
}
