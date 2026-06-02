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

    [Header("AI Feedback")]
    public StageAIFeedback aiFeedback = new StageAIFeedback { storyKey = "north" };

    [Header("Win Condition")]
    // Enter these exactly as they appear in the block's animationTriggerName
    public List<string> correctSequence = new List<string> {
        "Boil", "AddCoffee", "AddCardamom", "Pour"
    };
    public string successTrigger = "BoySuccess";
    private bool challengeCompleted;
    private bool successFeedbackPlayed;

    private void Awake()
    {
        submitButton = GetComponent<Button>() ?? submitButton;
        submitButton.onClick.AddListener(OnSubmitClicked);

        if (solutionSheet == null)
            solutionSheet = GameObject.Find("solution_sheet").transform;
    }

    private void Start()
    {
        if (aiFeedback != null)
        {
            aiFeedback.EnsureInitialized(this, "north");
        }

        if (AICompanionController.Instance != null)
        {
            AICompanionController.Instance.RequestStoryIntro();
        }
    }

    private void OnSubmitClicked()
    {
        if (challengeCompleted)
            return;

        StartCoroutine(PlayBlocksInOrder());
    }

    private IEnumerator PlayBlocksInOrder()
    {
        submitButton.interactable = false;

        List<CodingBlock> orderedBlocks = GetOrderedBlocks();

        // 1. First, check if the sequence is correct
        bool isCorrect = CheckIfSequenceIsCorrect(orderedBlocks);
        string feedbackSignature = BuildSequenceSignature(orderedBlocks);

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
            challengeCompleted = true;
        }
        else
        {
            Debug.Log("Sequence Incorrect or Incomplete.");
            aiFeedback.RequestWrong(
                this,
                "north",
                "north_submit:" + feedbackSignature,
                BuildSequenceFeedbackLine(orderedBlocks)
            );
        }

        submitButton.interactable = !challengeCompleted;

        if (challengeCompleted)
        {
            if (!successFeedbackPlayed)
            {
                successFeedbackPlayed = true;

                // 1. نطلب من المساعد نطق عبارة النجاح أولاً
                aiFeedback.RequestSuccess(
                    this,
                    "north",
                    "north_success",
                    "أحسنت، رتبت خطوات القهوة بالتسلسل الصحيح."
                );

                GameEvents.OnChallengeComplete?.Invoke();
            }

            if (AICompanionController.Instance != null)
            {
                AudioSource aiVoice = AICompanionController.Instance.GetComponentInChildren<AudioSource>();

                if (aiVoice != null)
                {
                    yield return new WaitForSeconds(0.5f);
                    while (aiVoice.isPlaying)
                    {
                        yield return null; // انتظر فريم بفريم حتى يسكت تماماً
                    }
                }
            }

            if (completionPopup != null)
            {
                completionPopup.ShowPopup();
                Debug.Log("[UI Flow] المساعد انتهى من الكلام.. تم إظهار بوب آب النجاح بنجاح!");
            }
        }
        else
        {
            // إذا كان الحل خاطئاً، ننتظر ثانية قبل إتاحة الزر مجدداً
            yield return new WaitForSeconds(1.0f);
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

    private string BuildSequenceFeedbackLine(List<CodingBlock> playerBlocks)
    {
        if (playerBlocks.Count == 0)
            return "ضع أول خطوة في تسلسل القهوة قبل الإتمام.";

        if (playerBlocks.Count < correctSequence.Count)
            return "الخطوات ناقصة؛ أكمل تسلسل القهوة قبل الإتمام.";

        if (playerBlocks.Count > correctSequence.Count)
            return "هناك خطوة زائدة؛ أبق خطوات القهوة الأساسية فقط.";

        int wrongIndex = GetFirstWrongSequenceIndex(playerBlocks);
        if (wrongIndex >= 0)
            return "الخطوة " + GetArabicStepLabel(wrongIndex) + " غير مناسبة في تسلسل القهوة.";

        return "راجع تسلسل القهوة خطوة بخطوة.";
    }

    private int GetFirstWrongSequenceIndex(List<CodingBlock> playerBlocks)
    {
        int countToCheck = Mathf.Min(playerBlocks.Count, correctSequence.Count);

        for (int i = 0; i < countToCheck; i++)
        {
            if (playerBlocks[i].animationTriggerName != correctSequence[i])
                return i;
        }

        return -1;
    }

    private string GetArabicStepLabel(int index)
    {
        switch (index)
        {
            case 0: return "الأولى";
            case 1: return "الثانية";
            case 2: return "الثالثة";
            case 3: return "الرابعة";
            default: return "الحالية";
        }
    }

    private string BuildSequenceSignature(List<CodingBlock> playerBlocks)
    {
        List<string> blockIds = new List<string>();

        foreach (CodingBlock block in playerBlocks)
            blockIds.Add(block != null ? block.animationTriggerName : "");

        return string.Join("|", blockIds);
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
