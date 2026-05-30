using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitManager : MonoBehaviour
{
    [Header("References")]
    public Transform solutionSheet;
    public Button submitButton;
    public Animator characterAnimator;

    [Header("Success UI")]
    public CompletionPopupController completionPopup; 

    [Header("Win Condition")]
    // Enter these exactly as they appear in the block's animationTriggerName
    public List<string> correctSequence = new List<string> { 
        "Boil", "AddCoffee", "AddCardamom", "Pour" 
    };
    public string successTrigger = "BoySuccess";

    private void Awake()
    {
        submitButton = GetComponent<Button>() ?? submitButton;
        submitButton.onClick.AddListener(OnSubmitClicked);

        if (solutionSheet == null)
            solutionSheet = GameObject.Find("solution_sheet").transform;
    }

    private void OnSubmitClicked()
    {
        StartCoroutine(PlayBlocksInOrder());
    }

    private IEnumerator PlayBlocksInOrder()
    {
        submitButton.interactable = false;

        List<CodingBlock> orderedBlocks = GetOrderedBlocks();
        
        // 1. First, check if the sequence is correct
        bool isCorrect = CheckIfSequenceIsCorrect(orderedBlocks);

        // 2. Play the animations in order
        foreach (CodingBlock block in orderedBlocks)
        {
            string triggerName = block.animationTriggerName;

            if (!string.IsNullOrEmpty(triggerName) && characterAnimator != null)
            {
                block.PlayActionSound();

                characterAnimator.SetTrigger(triggerName);

                yield return null;
                while (characterAnimator.IsInTransition(0)) yield return null;

                AnimatorStateInfo state = characterAnimator.GetCurrentAnimatorStateInfo(0);
                yield return new WaitForSeconds(state.length);
            }
        }

        // 3. If everything was correct, trigger the success animation at the end!
        if (isCorrect && orderedBlocks.Count == correctSequence.Count)
        {
            Debug.Log("Sequence Correct! Triggering Success.");
            characterAnimator.SetTrigger(successTrigger);
        }
        else
        {
            Debug.Log("Sequence Incorrect or Incomplete.");
        }

        submitButton.interactable = true;
        yield return new WaitForSeconds(1.0f);

        if (completionPopup != null)
        {
            completionPopup.ShowPopup();
        }
    }

    private bool CheckIfSequenceIsCorrect(List<CodingBlock> playerBlocks)
    {
        if (playerBlocks.Count != correctSequence.Count) return false;

        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (playerBlocks[i].animationTriggerName != correctSequence[i])
            {
                return false; 
            }
        }
        return true;
    }

    private List<CodingBlock> GetOrderedBlocks()
    {
        List<CodingBlock> result = new List<CodingBlock>();
        CodingBlock topBlock = null;

        foreach (Transform child in solutionSheet)
        {
            CodingBlock cb = child.GetComponent<CodingBlock>();
            if (cb != null && !cb.isTemplate)
            {
                topBlock = cb;
                break;
            }
        }

        if (topBlock == null) return result;

        CodingBlock current = topBlock;
        while (current != null)
        {
            result.Add(current);
            current = GetChildBlock(current);
        }
        return result;
    }

    private CodingBlock GetChildBlock(CodingBlock parent)
    {
        foreach (Transform child in parent.transform)
        {
            CodingBlock cb = child.GetComponent<CodingBlock>();
            if (cb != null && !cb.isTemplate) return cb;
        }
        return null;
    }
}