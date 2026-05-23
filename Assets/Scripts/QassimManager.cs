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

    private bool isSequenceActive = false;

    void Start()
    {
        textBubble.SetActive(false);
        LoadCustomerLevel(currentCustomerIndex);
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

        // Force animator back to Entry walk clip state loop
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
        if (isSequenceActive) return;

        if (CheckPlayerSolution())
        {
            StartCoroutine(SuccessRoutine());
        }
        else
        {
            StartCoroutine(FailureRoutine());
        }
    }

    [Header("Puzzle Settings")]
    public Transform solutionSheetPanel;

    private bool CheckPlayerSolution()
    {
        DraggableBlock[] allBlocks = solutionSheetPanel.GetComponentsInChildren<DraggableBlock>();

        bool khlasHas20 = false;
        bool sukkariHas30 = false;

        // --- LEVEL 1 VALIDATION (Customer 1: Khlas) ---
        if (currentCustomerIndex == 0)
        {
            foreach (DraggableBlock block in allBlocks)
            {
                // Checks only if IF Khlas contains the 20 Riyals block
                if (block.blockIdentity == BlockIdentity.Khlas)
                {
                    if (IsValueAttachedToSameContainer(block, BlockIdentity._20riyals))
                    {
                        khlasHas20 = true;
                    }
                }
            }

            Debug.Log($"[Level 1 Check] Khlas with 20: {khlasHas20} (Standalone Validation Passed)");

            // MODIFIED: Returns true based purely on the primary rule being correct
            return khlasHas20;
        }

        // --- LEVEL 2 VALIDATION (Customer 2: Sukkari) ---
        else if (currentCustomerIndex == 1)
        {
            foreach (DraggableBlock block in allBlocks)
            {
                // Must explicitly have an ELSE_IF container wrapping Sukkari -> 30
                if (block.blockIdentity == BlockIdentity.Sukkari)
                {
                    if (IsValueAttachedToSpecificContainer(block, BlockIdentity._30riyals, "ELSE_IF"))
                    {
                        sukkariHas30 = true;
                    }
                }
            }

            Debug.Log($"[Level 2 Check] Else If Sukkari with 30: {sukkariHas30}");
            return sukkariHas30;
        }

        return false;
    }

    // Helper: Validates standalone pure "ELSE" blocks strictly avoiding "ELSE_IF"
    private bool IsPrice30InsidePureElseContainer(DraggableBlock[] allBlocks)
    {
        foreach (DraggableBlock block in allBlocks)
        {
            // UPDATE: Make sure this looks for your exact 30 Riyals identity value
            if (block.blockIdentity == BlockIdentity._30riyals)
            {
                Transform parentContainer = block.transform.parent;
                while (parentContainer != null && parentContainer != solutionSheetPanel)
                {
                    // Matches the exact hierarchy name of your pure Else block
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

    // Helper: Validates if a value block shares an IfElse container structure
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

    // Helper: Targets a specific named container explicitly (e.g., "ELSE_IF")
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

        LoadCustomerLevel(currentCustomerIndex);
    }

    IEnumerator FailureRoutine()
    {
        isSequenceActive = true;
        boyAnimator.SetTrigger("IsSadBoy");
        yield return new WaitForSeconds(1.5f);
        boyAnimator.SetTrigger("Thinking");
        isSequenceActive = false;
    }
}