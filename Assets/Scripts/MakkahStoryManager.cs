using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// MakkahStoryManager: Handles the logic for the Makkah story scene, 
/// including Tawaf animation sequence, UI transitions, and character state management.
/// </summary>
public class MakkahStoryManager : MonoBehaviour
{
    [Header("Block & Logic References")]
    [Tooltip("The main programming block used by the child.")]
    public LoopBlockLogic loopBlock;

    [Header("Character Settings")]
    [Tooltip("The character that will move during Tawaf.")]
    public GameObject character;
    private SpriteRenderer characterRenderer;

    [Header("Path Animation Settings")]
    [Tooltip("Define path steps using empty Transforms (Position, Scale, Sprite).")]
    public List<Transform> tawafPathPoints;
    [Range(0.01f, 1f)]
    public float movementDelay = 0.15f;

    [Header("UI & Visual Feedback")]
    [Tooltip("The sprite that slides down to show the lap counter.")]
    public GameObject hangingBlockSprite;
    public TextMeshProUGUI lapCounterText;
    [Tooltip("The object that appears once the task is successfully completed.")]
    public GameObject successObject;
    [Tooltip("A fixed anchor point to spawn the success object.")]
    public Transform successSpawnPoint;

    [Header("Animation Curves")]
    public float slideDuration = 0.6f;
    public Vector3 hiddenPositionOffset = new Vector3(0, 5, 0);
    private Vector3 startPosition;

    [Header("Audio Settings")]
    [Tooltip("The audio source that plays Makkah ambiance or Talbiyah.")]
    public AudioSource tawafAudioSource;
    [Tooltip("Should the audio fade out smoothly at the end?")]
    public bool fadeAudioAtEnd = true;

    [Header("Success UI")]
    public CompletionPopupController completionPopup;

    [Header("Submit Button")]
    public Button itmamButton;

    [Header("AI Feedback")]
    public AICompanionController aiCompanion;
    public string aiStoryKey = "mecca";
    public string defaultAIGender = "male";

    [Header("AI Text Output")]
    [Tooltip("النص الموجود داخل text_box/Canvas/Text - RTLTMP لعرض التلميح المنطوق.")]
    public TextMeshProUGUI aiFeedbackText;

    private Coroutine tawafCoroutine;
    private Coroutine slideCoroutine;
    private bool aiFeedbackInitialized;
    private bool challengeCompleted;
    private bool successFeedbackPlayed;
    private bool successSequenceStarted;
    private string lastWrongFeedbackSignature = "";
    private string lastSubmitFeedbackSignature = "";
    private readonly HashSet<string> playedWrongFeedbackSignatures = new HashSet<string>();
    private static bool makkahCompletionLocked;

    void Start()
    {
        makkahCompletionLocked = false;

        // Cache references and set initial states
        if (character != null)
            characterRenderer = character.GetComponent<SpriteRenderer>();

        if (hangingBlockSprite != null)
        {
            startPosition = hangingBlockSprite.transform.position;
            // Hide the hanging block above the screen at start
            hangingBlockSprite.transform.position = startPosition + hiddenPositionOffset;
        }

        if (successObject != null)
            successObject.SetActive(false);

        CacheItmamButton();
        CacheAIFeedbackText();
        InitializeAIFeedback();
    }

    /// <summary>
    /// Called when the 'Itmam' (Submit) button is clicked.
    /// Validates the player's code before starting the animation.
    /// </summary>
    public void OnItmamClick()
    {
        if (challengeCompleted || successSequenceStarted || makkahCompletionLocked)
            return;

        LoopBlockLogic activeLoop = GetActiveLoop();
        aiCompanion = GetAICompanion();

        if (activeLoop == null)
        {
            MarkSubmitFeedback("submit:no-loop");
            SendWrongBlockFeedback(new List<string> { "لم يتم وضع بلوك التكرار" });
            return;
        }

        if (activeLoop != null)
        {
            // CHECK 1: Correct Sequence (Logic)
            if (activeLoop.IsSequenceCorrect())
            {
                Debug.Log("Logic Validated: Starting Tawaf.");

                makkahCompletionLocked = true;
                challengeCompleted = true;
                successSequenceStarted = true;
                lastWrongFeedbackSignature = "";
                lastSubmitFeedbackSignature = "";
                playedWrongFeedbackSignatures.Clear();
                SetItmamButtonInteractable(false);

                AICompanionController companion = GetAICompanion();
                if (companion != null)
                    companion.CancelPendingVoiceFeedback();
                AICompanionController.CancelAllPendingVoiceFeedback();

                Transform standingChild = character.transform.Find("StandingModel");
                if (standingChild != null)
                {
                    Destroy(standingChild.gameObject);
                }

                if (tawafCoroutine != null) StopCoroutine(tawafCoroutine);
                if (slideCoroutine != null) StopCoroutine(slideCoroutine);

                slideCoroutine = StartCoroutine(SlideBlock(true));
                tawafCoroutine = StartCoroutine(PerformTawaf());
            }
            // CHECK 2: Wrong Loop Count (مثلاً لم يكتب 7)
            else if (!activeLoop.IsInputCorrect())
            {
                Debug.LogWarning("Execution Failed: Incorrect Loop Number.");
                string submitSignature = BuildSubmitFeedbackSignature("wrong-number", activeLoop);
                MarkSubmitFeedback(submitSignature);
                SendWrongNumberFeedback(activeLoop);
            }
            // CHECK 3: Wrong Block Order (ترتيب البلوكات داخل التكرار خاطئ)
            else
            {
                Debug.LogWarning("Execution Failed: Invalid Sequence Order.");

                if (aiCompanion != null)
                {
                    // استخراج ترتيب البلوكات الحالي الموضوعة داخل اللوب لإرسالها للـ AI
                    List<string> currentOrder = new List<string>();
                    foreach (Transform child in activeLoop.transform.GetComponentsInChildren<Transform>())
                    {
                        string nameLower = child.name.ToLower();
                        if (child != activeLoop.transform && nameLower.Contains("block") && 
                            (nameLower.Contains("(clone)") || nameLower.Contains("_copy") || nameLower.Contains("copy")))
                        {
                            // تنظيف الاسم للعرض البرمجي البسيط
                            string cleanName = child.name.Replace("_Copy", "").Replace("(Clone)", "").Trim();
                            currentOrder.Add(cleanName);
                        }
                    }

                    // إذا كانت منطقة التكرار فارغة تماماً
                    if (currentOrder.Count == 0)
                    {
                        currentOrder.Add("مساحة تكرار فارغة");
                    }

                    string submitSignature = BuildSubmitFeedbackSignature(
                        "wrong-blocks",
                        activeLoop.GetIterationValue(),
                        currentOrder
                    );

                    MarkSubmitFeedback(submitSignature);
                    SendWrongBlockFeedback(currentOrder);
                }
            }
        }
    }

    public void OnLoopIterationEdited(LoopBlockLogic editedLoop)
    {
        if (editedLoop == null || challengeCompleted ||
            successSequenceStarted || makkahCompletionLocked)
            return;

        string enteredValue = editedLoop.GetIterationValue();

        if (string.IsNullOrWhiteSpace(enteredValue))
        {
            lastWrongFeedbackSignature = "";
            lastSubmitFeedbackSignature = "";
            return;
        }

        if (!editedLoop.IsInputCorrect())
            SendWrongNumberFeedback(editedLoop);
    }

    private LoopBlockLogic GetActiveLoop()
    {
        if (Drag.solutionSheet != null)
        {
            LoopBlockLogic activeLoop =
                Drag.solutionSheet.GetComponentInChildren<LoopBlockLogic>();

            if (activeLoop != null)
                return activeLoop;
        }

        return loopBlock;
    }

    private AICompanionController GetAICompanion()
    {
        if (aiCompanion != null)
            return aiCompanion;

        if (AICompanionController.Instance != null)
        {
            aiCompanion = AICompanionController.Instance;
            return aiCompanion;
        }

        aiCompanion = FindObjectOfType<AICompanionController>();
        if (aiCompanion == null)
            aiCompanion = gameObject.AddComponent<AICompanionController>();

        return aiCompanion;
    }

    private void InitializeAIFeedback()
    {
        AICompanionController companion = GetAICompanion();

        if (companion == null || aiFeedbackInitialized)
            return;

        companion.InitializeCompanion(aiStoryKey, ResolveAIGender());
        aiFeedbackInitialized = true;
    }

    private string ResolveAIGender()
    {
        return PlayerVoiceResolver.GetStoredGender(defaultAIGender);
    }

    private void SendWrongNumberFeedback(LoopBlockLogic activeLoop)
    {
        AICompanionController companion = GetAICompanion();

        if (companion == null || activeLoop == null)
            return;

        string enteredValue = activeLoop.GetIterationValue();
        if (string.IsNullOrWhiteSpace(enteredValue))
            enteredValue = "فارغ";

        string signature = "number:" + enteredValue.Trim();
        companion.CancelPendingVoiceFeedback();
        AICompanionController.CancelAllPendingVoiceFeedback();

        string feedbackLine = "عدد التكرار غير صحيح؛ راجع خانة التكرار.";
        ShowAIFeedbackText(feedbackLine);

        bool feedbackStarted = companion.RequestExactVoiceFeedback(
            "",
            feedbackLine,
            false,
            false
        );

        if (feedbackStarted)
        {
            MarkWrongFeedbackPlayed(signature);
            GameEvents.OnWrongAttempt?.Invoke(companion.GetAttemptCount());
        }
    }

    private void SendWrongBlockFeedback(LoopBlockLogic activeLoop)
    {
        List<string> currentOrder = activeLoop != null
            ? activeLoop.GetSortedNestedBlockNames()
            : new List<string>();

        if (currentOrder.Count == 0)
            currentOrder.Add("لم يتم وضع بلوكات داخل التكرار");

        SendWrongBlockFeedback(currentOrder);
    }

    private void SendWrongBlockFeedback(List<string> currentOrder)
    {
        AICompanionController companion = GetAICompanion();

        if (companion == null)
            return;

        if (currentOrder == null)
            currentOrder = new List<string>();

        string signature = "blocks:" + string.Join("|", currentOrder);
        companion.CancelPendingVoiceFeedback();
        AICompanionController.CancelAllPendingVoiceFeedback();

        string feedbackLine = "راجع ترتيب بلوكات الطواف داخل التكرار.";
        ShowAIFeedbackText(feedbackLine);

        bool feedbackStarted = companion.RequestExactVoiceFeedback(
            "",
            feedbackLine,
            false,
            false
        );

        if (feedbackStarted)
        {
            MarkWrongFeedbackPlayed(signature);
            GameEvents.OnWrongAttempt?.Invoke(companion.GetAttemptCount());
        }
    }

    private void SendSuccessFeedback()
    {
        if (successFeedbackPlayed)
            return;

        successFeedbackPlayed = true;

        AICompanionController companion = GetAICompanion();

        if (companion != null)
        {
            companion.CancelPendingVoiceFeedback();
            AICompanionController.CancelAllPendingVoiceFeedback();

            string successLine = "أحسنت، أكملت تكرار الطواف بنجاح.";
            ShowAIFeedbackText(successLine);

            if (companion.RequestExactVoiceFeedback(
                "makkah_success",
                successLine,
                true,
                true
            )) { }
        }

        GameEvents.OnChallengeComplete?.Invoke();
    }

    private bool IsRepeatedWrongFeedback(string signature)
    {
        return !string.IsNullOrEmpty(signature) &&
               (signature == lastWrongFeedbackSignature ||
                playedWrongFeedbackSignatures.Contains(signature));
    }

    private void MarkWrongFeedbackPlayed(string signature)
    {
        lastWrongFeedbackSignature = signature ?? "";
        if (!string.IsNullOrEmpty(signature))
            playedWrongFeedbackSignatures.Add(signature);
    }

    private string BuildSubmitFeedbackSignature(string resultType, LoopBlockLogic activeLoop)
    {
        List<string> currentOrder = activeLoop != null
            ? activeLoop.GetSortedNestedBlockNames()
            : new List<string>();

        string enteredValue = activeLoop != null ? activeLoop.GetIterationValue() : "";
        return BuildSubmitFeedbackSignature(resultType, enteredValue, currentOrder);
    }

    private string BuildSubmitFeedbackSignature(
        string resultType,
        string enteredValue,
        List<string> currentOrder)
    {
        return resultType + ":" +
               (enteredValue ?? "").Trim() + ":" +
               string.Join("|", currentOrder ?? new List<string>());
    }

    private bool IsRepeatedSubmitFeedback(string signature)
    {
        return !string.IsNullOrEmpty(signature) &&
               signature == lastSubmitFeedbackSignature;
    }

    private void MarkSubmitFeedback(string signature)
    {
        lastSubmitFeedbackSignature = signature ?? "";
    }

    private void CancelPendingAIFeedback()
    {
        AICompanionController companion = GetAICompanion();
        if (companion != null)
            companion.CancelPendingVoiceFeedback();
        AICompanionController.CancelAllPendingVoiceFeedback();
    }

    private void SetItmamButtonInteractable(bool interactable)
    {
        CacheItmamButton();

        Button targetButton = itmamButton;

        if (targetButton == null && EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null)
        {
            targetButton =
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        }

        if (targetButton != null)
        {
            itmamButton = targetButton;
            itmamButton.interactable = interactable;

            if (!interactable && EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject == itmamButton.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void CacheItmamButton()
    {
        if (itmamButton != null)
            return;

        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            int eventCount = button.onClick.GetPersistentEventCount();

            for (int i = 0; i < eventCount; i++)
            {
                if (button.onClick.GetPersistentTarget(i) == this &&
                    button.onClick.GetPersistentMethodName(i) == nameof(OnItmamClick))
                {
                    itmamButton = button;
                    return;
                }
            }
        }
    }

    private void CacheAIFeedbackText()
    {
        if (aiFeedbackText != null)
            return;

        GameObject textBox = GameObject.Find("text_box");
        if (textBox != null)
        {
            aiFeedbackText = textBox.GetComponentInChildren<TextMeshProUGUI>(true);
            if (aiFeedbackText != null)
                return;
        }

        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null || !text.gameObject.scene.IsValid())
                continue;

            if (text.gameObject.scene != gameObject.scene)
                continue;

            if (HasAncestorNamed(text.transform, "text_box"))
            {
                aiFeedbackText = text;
                return;
            }
        }
    }

    private void ShowAIFeedbackText(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        CacheAIFeedbackText();

        if (aiFeedbackText == null)
            return;

        ActivateHierarchy(aiFeedbackText.transform);
        aiFeedbackText.gameObject.SetActive(true);
        ArabicTextFormatter.ApplyTo(aiFeedbackText, message);
        Debug.Log("[Makkah AI Text] " + message);
    }

    private bool HasAncestorNamed(Transform child, string targetName)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.name == targetName)
                return true;

            current = current.parent;
        }

        return false;
    }

    private void ActivateHierarchy(Transform child)
    {
        Transform current = child;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
                current.gameObject.SetActive(true);

            if (current.name == "text_box")
                break;

            current = current.parent;
        }
    }

    /// <summary>
    /// Animates the hanging UI block sliding in or out of the scene.
    /// </summary>
    IEnumerator SlideBlock(bool show)
    {
        float elapsed = 0;
        Vector3 targetPos = show ? startPosition : startPosition + hiddenPositionOffset;
        Vector3 currentPos = hangingBlockSprite.transform.position;

        while (elapsed < slideDuration)
        {
            hangingBlockSprite.transform.position = Vector3.Lerp(currentPos, targetPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        hangingBlockSprite.transform.position = targetPos;
    }

    /// <summary>
    /// Core Coroutine: Cycles through 7 laps, updating character position and sprites.
    /// Ends with a cleanup and activation of the success feedback.
    /// </summary>
    IEnumerator PerformTawaf()
    {
        for (int i = 0; i < 7; i++)
        {
            if (tawafAudioSource != null)
            {
                tawafAudioSource.Play();
                tawafAudioSource.loop = true;
            }
            // Update Lap Counter UI
            if (lapCounterText != null)
                lapCounterText.text = (i + 1).ToString();

            // Iterate through each point on the Tawaf path
            foreach (Transform point in tawafPathPoints)
            {
                character.transform.position = point.position;
                character.transform.localScale = point.localScale;

                // Sync the character's sprite with the path point's sprite
                SpriteRenderer pointRenderer = point.GetComponent<SpriteRenderer>();
                if (characterRenderer != null && pointRenderer != null)
                {
                    characterRenderer.sprite = pointRenderer.sprite;
                }

                yield return new WaitForSeconds(movementDelay);
            }
        }

        // --- stop sound ---
        if (tawafAudioSource != null)
        {
            if (fadeAudioAtEnd)
                StartCoroutine(FadeOutAudio(1.5f)); 
            else
                tawafAudioSource.Stop();
        }

        // --- Sequence Completion ---
        slideCoroutine = StartCoroutine(SlideBlock(false));

        if (successObject != null)
        {
            character.SetActive(false);

            if (successSpawnPoint != null)
                successObject.transform.position = successSpawnPoint.position;
            else
                successObject.transform.position = new Vector3(1.5f, -2.0f, 0f);

            successObject.SetActive(true);

            if (lapCounterText != null)
                lapCounterText.text = "Done";

            Debug.Log("Story Completed: Activating completion popup next.");

            yield return new WaitForSeconds(1.0f);

            SendSuccessFeedback();

            if (completionPopup != null)
            {
                completionPopup.ShowPopup();
            }
            else
            {
                Debug.LogError("MakkahStoryManager: CompletionPopup reference is MISSING in the Inspector!");
            }
        }
    }


    /// <summary>
    /// Smoothly lowers the volume before stopping the audio.
    /// </summary>
    IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = tawafAudioSource.volume;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tawafAudioSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
            yield return null;
        }

        tawafAudioSource.Stop();
        tawafAudioSource.volume = startVolume;
    }

    /// <summary>
    /// نظام التحقق الصوتي التفاعلي في الوقت الفعلي أثناء قيام الطالب بحل اللغز وسحب البلوكات.
    /// </summary>
    public void CheckRealTimeSequence()
    {
        if (challengeCompleted || successSequenceStarted || makkahCompletionLocked)
            return;

        LoopBlockLogic activeLoop = GetActiveLoop();
        aiCompanion = GetAICompanion();

        if (activeLoop != null)
        {
            // استخراج البلوكات الحالية الموضوعة داخل اللوب
            List<string> currentOrder = new List<string>();
            List<Drag> dragBlocks = new List<Drag>();

            foreach (Transform child in activeLoop.transform.GetComponentsInChildren<Transform>())
            {
                string nameLower = child.name.ToLower();
                if (child != activeLoop.transform && nameLower.Contains("block") && 
                    (nameLower.Contains("(clone)") || nameLower.Contains("_copy") || nameLower.Contains("copy")))
                {
                    Drag blockDrag = child.GetComponent<Drag>();
                    if (blockDrag != null)
                    {
                        string cleanName = child.name.Replace("_Copy", "").Replace("(Clone)", "").Trim();
                        currentOrder.Add(cleanName);
                        dragBlocks.Add(blockDrag);
                    }
                }
            }

            if (currentOrder.Count == 0) return; // لا توجد بلوكات حالياً، لا يوجد خطأ

            // التحقق خطوة بخطوة في الوقت الفعلي
            for (int i = 0; i < currentOrder.Count; i++)
            {
                bool isCorrectStep = false;
                
                // البلوك الأول يجب أن يكون Block2 (حركة المشي)
                if (i == 0 && currentOrder[0].ToLower().Contains("block2")) isCorrectStep = true;
                
                // البلوك الثاني يجب أن يكون Block3 (الدعاء)
                if (i == 1 && currentOrder[1].ToLower().Contains("block3")) isCorrectStep = true;

                if (!isCorrectStep)
                {
                    // خطوة خاطئة! طرد البلوك فوراً وإعادته لموضعه الأصلي
                    Debug.LogWarning($"[Real-Time Validation] Wrong block placed at index {i}: {currentOrder[i]}. Ejecting!");
                    
                    Drag incorrectBlock = dragBlocks[i];
                    if (incorrectBlock != null)
                    {
                        incorrectBlock.transform.SetParent(null);
                        incorrectBlock.ResetToStartPos();
                    }

                    if (aiCompanion != null)
                    {
                        // إنشاء تلميح مخصص للترتيب الخاطئ حتى هذه اللحظة
                        List<string> cleanOrder = new List<string>();
                        for (int j = 0; j <= i; j++) cleanOrder.Add(currentOrder[j]);
                        
                        SendWrongBlockFeedback(cleanOrder);
                    }

                    break; // التوقف فور اكتشاف أول خطأ وطرد البلوك الخاطئ
                }
            }
        }
    }
}
