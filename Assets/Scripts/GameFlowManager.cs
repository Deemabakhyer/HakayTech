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

    private bool animationStarted = false;

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
            storyScenePlayer.PlayStory();
        else
            Debug.Log("StoryScenePlayer is missing");
    }
}