using UnityEngine;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    [Header("Blocks")]
    public Transform droppedBlocksArea;

    [Header("Popups")]
    public GameObject dimBackground;
    public GameObject codeFunctionPopup;
    public GameObject nameFunctionPopup;

    [Header("Input")]
    public TMP_InputField nameInputField;

    [Header("Animation")]
    public StoryScenePlayer storyScenePlayer;

    [Header("AI Dialogue")]
    public SouthAIDialogueController aiDialogue;

    [Header("Success UI Popup")]
    public CompletionPopupController completionPopup;

    [Header("AI Feedback")]
    public StageAIFeedback aiFeedback = new StageAIFeedback { storyKey = "south" };

    private bool animationStarted = false;
    private bool successFeedbackPlayed = false;

    private readonly string[] correctOrder =
    {
        "define",
        "flour",
        "ghee",
        "honey",
        "mix",
        "serve",
        "call"
    };

    public void OnCompleteButtonClicked()
    {
        if (animationStarted)
            return;

        if (IsAnyPopupOpen())
            return;

        if (IsBlocksOrderCorrect())
        {
            if (dimBackground != null)
                dimBackground.SetActive(true);

            if (codeFunctionPopup != null)
                codeFunctionPopup.SetActive(true);

            if (GameUISoundManager.Instance != null)
                GameUISoundManager.Instance.PlayPopupOpen();

            if (aiDialogue != null)
                aiDialogue.ShowWriteFunctionName();
        }
        else
        {
            aiFeedback.RequestWrong(
                this,
                "south",
                "south_order:" + BuildDroppedOrderSignature(),
                BuildOrderFeedbackLine()
            );

            if (aiDialogue != null)
                aiDialogue.ShowWrongOrder();

            UIShakeEffect shake = droppedBlocksArea.GetComponent<UIShakeEffect>();

            if (shake != null)
                shake.PlayShake();
        }
    }

    private bool IsBlocksOrderCorrect()
    {
        if (droppedBlocksArea == null)
            return false;

        if (droppedBlocksArea.childCount != correctOrder.Length)
            return false;

        for (int i = 0; i < correctOrder.Length; i++)
        {
            BlockDragHandler block =
                droppedBlocksArea.GetChild(i).GetComponent<BlockDragHandler>();

            if (block == null || block.blockID != correctOrder[i])
                return false;
        }

        return true;
    }

    public void ShowNamePopup()
    {
        if (animationStarted)
            return;

        if (codeFunctionPopup != null)
            codeFunctionPopup.SetActive(false);

        if (dimBackground != null)
            dimBackground.SetActive(true);

        if (nameFunctionPopup != null)
            nameFunctionPopup.SetActive(true);

        if (GameUISoundManager.Instance != null)
            GameUISoundManager.Instance.PlayPopupOpen();

        if (aiDialogue != null)
            aiDialogue.ShowWriteFunctionName();
    }

    public void ConfirmFunctionName()
    {
        if (animationStarted)
            return;

        if (nameInputField == null)
            return;

        string input = nameInputField.text.Trim().ToLower();

        if (input == "make_areeka" || input == "make_areeka()")
        {
            CloseAllPopups();

            if (aiDialogue != null)
                aiDialogue.ShowRunFunction();

            animationStarted = true;
            StartAnimation();
        }
        else
        {
            aiFeedback.RequestWrong(
                this,
                "south",
                "south_function_name:" + input,
                "اسم الدالة غير مطابق؛ اكتب اسم الاستدعاء نفسه."
            );

            if (aiDialogue != null)
                aiDialogue.ShowWrongPlace();

            UIShakeEffect shake = nameFunctionPopup.GetComponent<UIShakeEffect>();

            if (shake != null)
                shake.PlayShake();
        }
    }

    public void CloseAllPopups()
    {
        if (codeFunctionPopup != null)
            codeFunctionPopup.SetActive(false);

        if (nameFunctionPopup != null)
            nameFunctionPopup.SetActive(false);

        if (dimBackground != null)
            dimBackground.SetActive(false);
    }

    private bool IsAnyPopupOpen()
    {
        bool codeOpen = codeFunctionPopup != null && codeFunctionPopup.activeSelf;
        bool nameOpen = nameFunctionPopup != null && nameFunctionPopup.activeSelf;

        return codeOpen || nameOpen;
    }

    private void StartAnimation()
    {
        if (storyScenePlayer != null)
        {
            storyScenePlayer.PlayStory(OnStoryAnimationCompleted);
        }
        else
        {
            Debug.Log("StoryScenePlayer is missing");
            OnStoryAnimationCompleted();

            if (completionPopup != null)
                completionPopup.ShowPopup();
        }
    }

    private void OnStoryAnimationCompleted()
    {
        if (successFeedbackPlayed)
            return;

        successFeedbackPlayed = true;
        aiFeedback.RequestSuccess(
            this,
            "south",
            "south_success",
            "أحسنت، رتبت الدالة واستدعيتها بنجاح."
        );
        GameEvents.OnChallengeComplete?.Invoke();
    }

    private string BuildDroppedOrderSignature()
    {
        if (droppedBlocksArea == null)
            return "no-drop-area";

        string[] order = new string[droppedBlocksArea.childCount];

        for (int i = 0; i < droppedBlocksArea.childCount; i++)
        {
            BlockDragHandler block =
                droppedBlocksArea.GetChild(i).GetComponent<BlockDragHandler>();

            order[i] = block != null ? block.blockID : "";
        }

        return string.Join("|", order);
    }

    private string BuildOrderFeedbackLine()
    {
        if (droppedBlocksArea == null || droppedBlocksArea.childCount == 0)
            return "ضع أول خطوة داخل الدالة قبل الإتمام.";

        int wrongIndex = GetFirstWrongOrderIndex();
        if (wrongIndex >= 0 && wrongIndex < correctOrder.Length)
            return "الخطوة " + GetArabicStepLabel(wrongIndex) + " غير مناسبة داخل الدالة.";

        if (droppedBlocksArea.childCount < correctOrder.Length)
            return "خطوات الدالة ناقصة؛ أكملها قبل الإتمام.";

        if (droppedBlocksArea.childCount > correctOrder.Length)
            return "هناك خطوة زائدة داخل الدالة؛ أزلها.";

        return "راجع ترتيب خطوات الدالة قبل الإتمام.";
    }

    private int GetFirstWrongOrderIndex()
    {
        if (droppedBlocksArea == null)
            return -1;

        int countToCheck = Mathf.Min(droppedBlocksArea.childCount, correctOrder.Length);

        for (int i = 0; i < countToCheck; i++)
        {
            BlockDragHandler block =
                droppedBlocksArea.GetChild(i).GetComponent<BlockDragHandler>();

            if (block == null || block.blockID != correctOrder[i])
                return i;
        }

        if (droppedBlocksArea.childCount > correctOrder.Length)
            return correctOrder.Length;

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
            case 4: return "الخامسة";
            case 5: return "السادسة";
            case 6: return "السابعة";
            default: return "الحالية";
        }
    }
}
