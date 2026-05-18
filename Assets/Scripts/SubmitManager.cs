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

    [Header("Story Props")]
    public GameObject coffeeBeans;
    public GameObject bakrajPot;     // Default pot on the boy's right
    public GameObject bakrajOnFire;  // Pot on the fire
    public GameObject hailBox;

    [Header("Win Condition")]
    public List<string> correctSequence = new List<string> {
        "MovePotToFire", "AddCoffee", "AddCardamom", "Pour"
    };
    public string successTrigger = "success";
    public string failTrigger = "fail";

    private void Awake()
    {
        submitButton = GetComponent<Button>() ?? submitButton;
        submitButton.onClick.AddListener(OnSubmitClicked);

        if (solutionSheet == null)
            solutionSheet = GameObject.Find("solution_sheet").transform;

        if (bakrajOnFire != null) bakrajOnFire.SetActive(false);
    }

    private void OnSubmitClicked()
    {
        StartCoroutine(PlayBlocksInOrder());
    }

    private IEnumerator PlayBlocksInOrder()
    {
        submitButton.interactable = false;
        List<CodingBlock> orderedBlocks = GetOrderedBlocks();

        bool isCorrect = CheckIfSequenceIsCorrect(orderedBlocks);

        // 1. Play each action block strictly one after another
        foreach (CodingBlock block in orderedBlocks)
        {
            string triggerName = block.animationTriggerName;

            if (!string.IsNullOrEmpty(triggerName) && characterAnimator != null)
            {
                Debug.Log($"[SubmitManager] Processing Action: {triggerName}");
                block.PlayActionSound();
                characterAnimator.SetTrigger(triggerName);

                // --- INLINE PROP HANDLING PER BLOCK TYPE ---
                if (triggerName == "MovePotToFire")
                {
                    // Pot on right disappears immediately when he starts moving it
                    if (bakrajPot != null) bakrajPot.SetActive(false);

                    // Wait for the duration of the boil animation state to finish completely
                    yield return StartCoroutine(WaitForCurrentAnimation());

                    // Pot on fire appears ONLY after the boiling movement state is complete
                    if (bakrajOnFire != null) bakrajOnFire.SetActive(true);
                }
                else if (triggerName == "AddCoffee")
                {
                    // Wait a tiny fraction for his hand to grab it, then hide the beans
                    yield return new WaitForSeconds(0.3f);
                    if (coffeeBeans != null) coffeeBeans.SetActive(false);

                    // Wait for the remaining duration of the add coffee clip
                    yield return StartCoroutine(WaitForCurrentAnimation());
                }
                else if (triggerName == "AddCardamom")
                {
                    // Wait a tiny fraction for his hand to grab it, then hide the hail box
                    yield return new WaitForSeconds(0.3f);
                    if (hailBox != null) hailBox.SetActive(false);

                    // Wait for the remaining duration of the add cardamom clip
                    yield return StartCoroutine(WaitForCurrentAnimation());
                }
                else // For "Pour" or any other blocks
                {
                    yield return StartCoroutine(WaitForCurrentAnimation());
                }
            }
        }

        // 2. Play Success / Failure Evaluation
        if (characterAnimator != null && orderedBlocks.Count > 0)
        {
            if (isCorrect && orderedBlocks.Count == correctSequence.Count)
            {
                Debug.Log("[SubmitManager] Success Sequence Completed.");
                characterAnimator.SetTrigger(successTrigger);
            }
            else
            {
                Debug.Log("[SubmitManager] Sequence incomplete/incorrect.");
                characterAnimator.SetTrigger(failTrigger);
            }

            yield return StartCoroutine(WaitForCurrentAnimation());
        }

        // 3. Reset everything back to defaults once sequence is completely done
        ResetAllProps();
        submitButton.interactable = true;
    }

    // A robust helper function that dynamically tracks animation changes
    private IEnumerator WaitForCurrentAnimation()
    {
        // 1. Wait out the current frame so Unity registers the code's trigger input
        yield return new WaitForEndOfFrame();

        // 2. If a transition is actively occurring, wait for it to land on the destination clip
        while (characterAnimator.IsInTransition(0))
        {
            yield return null;
        }

        // 3. Grab the active state metadata and wait out its precise length
        AnimatorStateInfo state = characterAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);
    }

    private void ResetAllProps()
    {
        if (bakrajPot != null) bakrajPot.SetActive(true);
        if (coffeeBeans != null) coffeeBeans.SetActive(true);
        if (hailBox != null) hailBox.SetActive(true);
        if (bakrajOnFire != null) bakrajOnFire.SetActive(false);
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