using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPathController : MonoBehaviour
{
    [Header("Player")]
    public RectTransform playerCharacter;

    [Header("Waypoints")]
    public RectTransform pointStart;
    public RectTransform pointForward1;   // حطيها عند نهاية خطوتين للأمام
    public RectTransform pointRight;
    public RectTransform pointStop;       // اختياري
    public RectTransform pointLeft;
    public RectTransform pointForward2;
    public RectTransform pointEnd;

    [Header("Movement")]
    public float moveDuration = 0.55f;
    public float popScale = 1.08f;

    [Header("Stop Settings")]
    public bool useStopPoint = false;     // خليه OFF عشان التوقف ما يطير

    private Coroutine moveRoutine;
    private string lastSignature = "";

    public void SyncWithBlocks(List<DragBlock> placedBlocks, List<string> correctOrder)
    {
        if (playerCharacter == null || pointStart == null)
            return;

        string signature = BuildSignature(placedBlocks);

        if (signature == lastSignature)
            return;

        lastSignature = signature;

        RectTransform target = GetTargetFromBlocks(placedBlocks, correctOrder);

        if (target != null)
            MoveTo(target);
    }

    private RectTransform GetTargetFromBlocks(List<DragBlock> placedBlocks, List<string> correctOrder)
    {
        if (placedBlocks == null || placedBlocks.Count == 0)
            return pointStart;

        int validCount = GetValidPrefixCount(placedBlocks, correctOrder);

        if (validCount <= 0)
            return pointStart;

        RectTransform currentTarget = pointStart;
        int moveForwardCount = 0;

        for (int i = 0; i < validCount; i++)
        {
            string id = placedBlocks[i].blockID.Trim();

            switch (id)
            {
                case "Start":
                    currentTarget = pointStart;
                    break;

                case "MoveForward":
                    moveForwardCount++;

                    if (moveForwardCount == 1)
                        currentTarget = pointForward1;
                    else
                        currentTarget = pointForward2;

                    break;

                case "TurnRight":
                    currentTarget = pointRight;
                    break;

                case "Stop":
                    if (useStopPoint && pointStop != null)
                        currentTarget = pointStop;
                    // إذا useStopPoint = false، يبقى مكانه ولا يطير
                    break;

                case "TurnLeft":
                    currentTarget = pointLeft;
                    break;

                case "End":
                    currentTarget = pointEnd;
                    break;
            }
        }

        return currentTarget;
    }

    private int GetValidPrefixCount(List<DragBlock> placedBlocks, List<string> correctOrder)
    {
        int count = 0;

        for (int i = 0; i < placedBlocks.Count; i++)
        {
            if (i >= correctOrder.Count)
                break;

            string playerID = placedBlocks[i].blockID.Trim();
            string correctID = correctOrder[i].Trim();

            if (playerID != correctID)
                break;

            count++;
        }

        return count;
    }

    private string BuildSignature(List<DragBlock> placedBlocks)
    {
        if (placedBlocks == null || placedBlocks.Count == 0)
            return "EMPTY";

        string result = "";

        for (int i = 0; i < placedBlocks.Count; i++)
        {
            if (placedBlocks[i] != null)
                result += placedBlocks[i].blockID.Trim() + "|";
        }

        return result;
    }

    private void MoveTo(RectTransform target)
    {
        if (target == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(RectTransform target)
    {
        Vector3 startPos = playerCharacter.position;
        Vector3 endPos = target.position;

        Vector3 originalScale = playerCharacter.localScale;
        Vector3 popTarget = originalScale * popScale;

        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / moveDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, p);

            playerCharacter.position = Vector3.Lerp(startPos, endPos, smooth);

            if (p < 0.5f)
                playerCharacter.localScale = Vector3.Lerp(originalScale, popTarget, p * 2f);
            else
                playerCharacter.localScale = Vector3.Lerp(popTarget, originalScale, (p - 0.5f) * 2f);

            yield return null;
        }

        playerCharacter.position = endPos;
        playerCharacter.localScale = originalScale;
        moveRoutine = null;
    }
}