using UnityEngine;
using UnityEngine.UI;

public class IntroPopupHandler : MonoBehaviour
{
    public GameObject introPopup;
    public GameObject dimBackground;
    public Button startButton;

    [Header("AI Dialogue")]
    public SouthAIDialogueController aiDialogue;

    private void Awake()
    {
        ShowIntroPopup();
    }

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(CloseIntroPopup);
            startButton.onClick.AddListener(CloseIntroPopup);
        }
    }

    private void ShowIntroPopup()
    {
        if (dimBackground != null)
            dimBackground.SetActive(true);

        if (introPopup != null)
            introPopup.SetActive(true);

        if (aiDialogue != null)
            aiDialogue.ShowIntroExplain();

        if (GameUISoundManager.Instance != null)
            GameUISoundManager.Instance.PlayPopupOpen();
    }

    public void CloseIntroPopup()
    {
        // إيقاف نطق القصة الصوتي فوراً عند بدء اللعب لتفادي أي تداخل صوتي
        if (AICompanionController.Instance != null)
        {
            AICompanionController.Instance.StopSpeech();
        }
        AICompanionController.CancelAllPendingVoiceFeedback();

        if (introPopup != null)
            introPopup.SetActive(false);

        if (dimBackground != null)
            dimBackground.SetActive(false);

        if (aiDialogue != null)
            aiDialogue.ShowDragBlocks();
    }
}
