using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class QassimManager : MonoBehaviour
{
    [Header("Animators")]
    public Animator customerAnimator;
    public Animator boyAnimator;

    [Header("UI Character Elements")]
    public RectTransform customerRT;
    public Image customerImage;
    public GameObject textBubble;
    public TextMeshProUGUI bubbleText;

    [Header("Movement Positions")]
    public RectTransform startPosition;
    public RectTransform stopPosition;
    public RectTransform exitPosition;
    public float moveSpeed = 300f;

    [Header("Customer Level Configuration")]
    public List<CustomerData> customerLevels = new List<CustomerData>();
    private int currentCustomerIndex = 0;

    [Header("Success UI Popup")]
    public CompletionPopupController completionPopup;

    [Header("AI Feedback")]
    public StageAIFeedback aiFeedback = new StageAIFeedback { storyKey = "qassim" };

    private bool isSequenceActive = false;
    private bool storyCompleted = false;

    void Start()
    {
        textBubble.SetActive(false);
        LoadCustomerLevel(currentCustomerIndex);

        if (aiFeedback != null)
        {
            aiFeedback.EnsureInitialized(this, "qassim");
        }

        if (AICompanionController.Instance != null)
        {
            AICompanionController.Instance.RequestStoryIntro();
        }
    }

    void LoadCustomerLevel(int index)
    {
        if (index >= customerLevels.Count)
        {
            Debug.Log("All customer stories completed successfully!");
            return;
        }

        CustomerData data = customerLevels[index];

        if (customerImage != null && data.customerSprite != null)
            customerImage.sprite = data.customerSprite;

        if (customerAnimator != null && data.animatorCtrl != null)
            customerAnimator.runtimeAnimatorController = data.animatorCtrl;

        customerAnimator.Play("Walking", 0, 0f);

        if (bubbleText != null)
            bubbleText.text = data.orderText;

        customerRT.anchoredPosition = startPosition.anchoredPosition;

        StartCoroutine(CustomerArrivalRoutine());
    }

    IEnumerator CustomerArrivalRoutine()
    {
        isSequenceActive = true;

        while (Vector2.Distance(customerRT.anchoredPosition, stopPosition.anchoredPosition) > 1f)
        {
            customerRT.anchoredPosition = Vector2.MoveTowards(
                customerRT.anchoredPosition,
                stopPosition.anchoredPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        customerAnimator.SetTrigger("StopWalk");
        yield return new WaitForSeconds(0.5f);

        textBubble.SetActive(true);
        customerAnimator.SetTrigger("standing");

        boyAnimator.SetTrigger("Thinking");
        isSequenceActive = false;
    }

    public void OnSubmitButtonPressed()
    {
        if (isSequenceActive || storyCompleted) return;

        if (CheckPlayerSolution())
        {
            StartCoroutine(SuccessRoutine());
        }
        else
        {
            aiFeedback.RequestWrong(
                this,
                "qassim",
                "qassim_submit_" + currentCustomerIndex + ":" + BuildSolutionSignature(),
                BuildCustomerFeedbackLine()
            );
            StartCoroutine(FailureRoutine());
        }
    }

    private string BuildCustomerFeedbackLine()
    {
        if (currentCustomerIndex == 0)
            return "حاوية إذا تحتاج شرط الإخلاص وسعره معًا.";

        if (currentCustomerIndex == 1)
            return "حاوية وإلا إذا تحتاج السكري وسعره معًا.";

        return "راجع الشرط والسعر داخل الحاوية المناسبة.";
    }

    [Header("Puzzle Settings")]
    public Transform solutionSheetPanel;

    private bool CheckPlayerSolution()
    {
        DraggableBlock[] allBlocks = solutionSheetPanel.GetComponentsInChildren<DraggableBlock>();

        bool khlasHas20 = false;
        bool sukkariHas30 = false;

        if (currentCustomerIndex == 0)
        {
            foreach (DraggableBlock block in allBlocks)
            {
                if (block.blockIdentity == BlockIdentity.Khlas)
                {
                    if (IsValueAttachedToSameContainer(block, BlockIdentity._20riyals))
                    {
                        khlasHas20 = true;
                    }
                }
            }
            return khlasHas20;
        }
        else if (currentCustomerIndex == 1)
        {
            foreach (DraggableBlock block in allBlocks)
            {
                if (block.blockIdentity == BlockIdentity.Sukkari)
                {
                    if (IsValueAttachedToSpecificContainer(block, BlockIdentity._30riyals, "ELSE_IF"))
                    {
                        sukkariHas30 = true;
                    }
                }
            }
            return sukkariHas30;
        }

        return false;
    }

    private bool IsPrice30InsidePureElseContainer(DraggableBlock[] allBlocks)
    {
        foreach (DraggableBlock block in allBlocks)
        {
            if (block.blockIdentity == BlockIdentity._30riyals)
            {
                Transform parentContainer = block.transform.parent;
                while (parentContainer != null && parentContainer != solutionSheetPanel)
                {
                    if (parentContainer.gameObject.name == "ELSE")
                    {
                        return true;
                    }
                    parentContainer = parentContainer.parent;
                }
            }
        }
        return false;
    }

    private bool IsValueAttachedToSameContainer(DraggableBlock conditionBlock, BlockIdentity targetValue)
    {
        Transform container = conditionBlock.transform.parent;
        while (container != null && container != solutionSheetPanel)
        {
            DraggableBlock containerBlock = container.GetComponent<DraggableBlock>();
            if (containerBlock != null && containerBlock.blockType == BlockType2.IfElse)
            {
                DraggableBlock[] containerChildren = container.GetComponentsInChildren<DraggableBlock>();
                foreach (DraggableBlock child in containerChildren)
                {
                    if (child.blockIdentity == targetValue) return true;
                }
            }
            container = container.parent;
        }
        return false;
    }

    private bool IsValueAttachedToSpecificContainer(DraggableBlock conditionBlock, BlockIdentity targetValue, string containerName)
    {
        Transform container = conditionBlock.transform.parent;
        while (container != null && container != solutionSheetPanel)
        {
            if (container.gameObject.name == containerName)
            {
                DraggableBlock[] containerChildren = container.GetComponentsInChildren<DraggableBlock>();
                foreach (DraggableBlock child in containerChildren)
                {
                    if (child.blockIdentity == targetValue) return true;
                }
            }
            container = container.parent;
        }
        return false;
    }

    IEnumerator SuccessRoutine()
    {
        isSequenceActive = true;
        textBubble.SetActive(false);

        boyAnimator.SetTrigger("TellingPrice");
        yield return new WaitForSeconds(1.5f);

        boyAnimator.SetTrigger("IsHappyBoy");
        customerAnimator.SetTrigger("IsHappy");

        yield return new WaitForSeconds(2.0f);

        while (Vector2.Distance(customerRT.anchoredPosition, exitPosition.anchoredPosition) > 1f)
        {
            customerRT.anchoredPosition = Vector2.MoveTowards(
                customerRT.anchoredPosition,
                exitPosition.anchoredPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        currentCustomerIndex++;
        isSequenceActive = false;
        aiFeedback.ClearWrongRepeat();

        if (currentCustomerIndex >= customerLevels.Count)
        {
            storyCompleted = true;
            Debug.Log("Qassim Story Fully Completed! Triggering Firebase & Completion Popup...");

            aiFeedback.RequestSuccess(
                this,
                "qassim",
                "qassim_success",
                "أحسنت، أنهيت طلبات القصيم بنجاح."
            );

            GameEvents.OnChallengeComplete?.Invoke();

            if (AICompanionController.Instance != null)
            {
                AudioSource aiVoice = AICompanionController.Instance.GetComponentInChildren<AudioSource>();
                if (aiVoice != null)
                {
                    yield return new WaitForSeconds(0.5f);
                    while (aiVoice.isPlaying)
                    {
                        yield return null;
                    }
                }
            }

            if (completionPopup != null)
            {
                completionPopup.ShowPopup();
            }
            else
            {
                Debug.LogError("QassimManager: CompletionPopup Reference is MISSING in the Inspector!");
            }
        }
        else
        {
            LoadCustomerLevel(currentCustomerIndex);
        }
    }

    IEnumerator FailureRoutine()
    {
        isSequenceActive = true;
        boyAnimator.SetTrigger("IsSadBoy");
        yield return new WaitForSeconds(1.5f);
        boyAnimator.SetTrigger("Thinking");
        isSequenceActive = false;
    }

    private string BuildSolutionSignature()
    {
        if (solutionSheetPanel == null)
            return "no-solution-sheet";

        DraggableBlock[] blocks = solutionSheetPanel.GetComponentsInChildren<DraggableBlock>();
        List<string> parts = new List<string>();

        foreach (DraggableBlock block in blocks)
        {
            if (block == null || block.isTemplate)
                continue;

            string parentName = block.transform.parent != null
                ? block.transform.parent.name
                : "";

            parts.Add(block.blockIdentity + "@" + parentName);
        }

        return string.Join("|", parts);
    }
}