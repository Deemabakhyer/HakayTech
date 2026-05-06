using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeSequenceController : MonoBehaviour
{
    public Transform selectedBlocksPanel;
    public List<string> correctOrder = new List<string>();

    public AIDialogueController aiDialogueController;

    public GameObject successConfetti;

    private bool isWaitingForFix = false;
    private int wrongIndex = -1;

    public void RunSequence()
    {
        if (!isWaitingForFix)
            StartCoroutine(RunSequenceRoutine());
        else
            StartCoroutine(ContinueAfterFixRoutine());
    }

    IEnumerator RunSequenceRoutine()
    {
        ResetAllBlocks();

        List<DragBlock> playerBlocks = GetBlocksInOrder();

        if (playerBlocks.Count == 0)
            yield break;

        if (aiDialogueController != null)
            aiDialogueController.ShowStartMessage();

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < playerBlocks.Count && i < correctOrder.Count; i++)
        {
            string current = playerBlocks[i].blockID;
            string expected = correctOrder[i];

            yield return new WaitForSeconds(0.6f);

            if (current != expected)
            {
                wrongIndex = i;
                isWaitingForFix = true;

                BlockEffects effects = playerBlocks[i].GetComponent<BlockEffects>();
                if (effects != null)
                    effects.ShowWrongEffect();

                if (aiDialogueController != null)
                    aiDialogueController.ShowErrorMessage();

                yield break;
            }
        }

        if (playerBlocks.Count != correctOrder.Count)
        {
            isWaitingForFix = true;

            if (aiDialogueController != null)
                aiDialogueController.ShowErrorMessage();

            yield break;
        }

        if (aiDialogueController != null)
            aiDialogueController.ShowSuccessMessage();

        if (successConfetti != null)
            successConfetti.SetActive(true);
    }

    IEnumerator ContinueAfterFixRoutine()
    {
        List<DragBlock> playerBlocks = GetBlocksInOrder();

        if (wrongIndex < 0 || wrongIndex >= playerBlocks.Count)
            yield break;

        if (playerBlocks[wrongIndex].blockID != correctOrder[wrongIndex])
        {
            if (aiDialogueController != null)
                aiDialogueController.ShowErrorMessage();

            yield break;
        }

        BlockEffects fixedBlock = playerBlocks[wrongIndex].GetComponent<BlockEffects>();
        if (fixedBlock != null)
            fixedBlock.ResetBlock();

        isWaitingForFix = false;

        yield return StartCoroutine(RunSequenceRoutine());
    }

    List<DragBlock> GetBlocksInOrder()
    {
        List<DragBlock> result = new List<DragBlock>();

        foreach (Transform child in selectedBlocksPanel)
        {
            DragBlock dragBlock = child.GetComponent<DragBlock>();
            if (dragBlock != null && !dragBlock.isTemplate)
                result.Add(dragBlock);
        }

        return result;
    }

    void ResetAllBlocks()
    {
        foreach (Transform child in selectedBlocksPanel)
        {
            BlockEffects effects = child.GetComponent<BlockEffects>();
            if (effects != null)
                effects.ResetBlock();
        }

        if (successConfetti != null)
            successConfetti.SetActive(false);
    }
}