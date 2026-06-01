using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlockValidationManager : MonoBehaviour
{
    // -------------------------------------------------------
    // المراجع الأساسية
    // -------------------------------------------------------
    [Header("References")]
    [Tooltip("مرجع AICompanionController في المشهد")]
    public AICompanionController aiCompanion;

    // -------------------------------------------------------
    // منطقة إسقاط البلوكات
    // -------------------------------------------------------
    [Header("Block Drop Zone")]
    [Tooltip("Transform التي تحتوي على البلوكات المرتّبة من الطفل")]
    public Transform blockDropZone;

    // -------------------------------------------------------
    // الترتيب الصحيح
    // -------------------------------------------------------
    private List<string> _correctBlockOrder = new List<string>();
    private string _storyKey = "stage";
    private string _lastFeedbackSignature = "";
    private readonly HashSet<string> _playedWrongFeedbackSignatures = new HashSet<string>();
    private bool _successFeedbackPlayed;

    // ===============================================================
    // تحميل بيانات القصة
    // ===============================================================
    public void LoadStoryData(StoryData data)
    {
        if (data == null)
        {
            Debug.LogError("[BlockValidation] StoryData فارغ!");
            return;
        }

        _correctBlockOrder = new List<string>(data.correctBlockOrder);
        _storyKey = data.storyKey;
        _lastFeedbackSignature = "";
        _playedWrongFeedbackSignatures.Clear();
        _successFeedbackPlayed = false;

        if (aiCompanion != null)
        {
            aiCompanion.InitializeCompanion(data.storyKey, data.playerGender);
            aiCompanion.SetCorrectBlockOrder(_correctBlockOrder);
            aiCompanion.RequestStoryIntro();
        }
        else
        {
            Debug.LogWarning("[BlockValidation] aiCompanion غير مسند في الـ Inspector!");
        }

        Debug.Log("[BlockValidation] تم تحميل القصة: " + data.storyKey);
    }

    // ===============================================================
    // نقطة الدخول — تُستدعى فور إفلات أي بلوكة
    // ===============================================================
    public void OnBlockDropped()
    {
        StartCoroutine(ValidateStepCoroutine());
    }

    // ===============================================================
    // منطق التحقق الكامل (ترتيب + محتوى)
    // ===============================================================
    private IEnumerator ValidateStepCoroutine()
    {
        // انتظار إطار واحد حتى يستقر البلوك في الـ Hierarchy
        yield return new WaitForEndOfFrame();

        List<string> currentOrder = GetCurrentBlockOrder();
        if (currentOrder.Count == 0) yield break;

        int lastIndex = currentOrder.Count - 1;

        // حماية: تجاوز العدد المطلوب
        if (lastIndex >= _correctBlockOrder.Count)
        {
            EjectLastBlock();
            SendWrongFeedback(
                "overflow:" + string.Join("|", currentOrder),
                "هناك بلوك زائد؛ أبق عدد الخطوات المطلوب فقط."
            );
            yield break;
        }

        // -------------------------------------------------------
        // الخطوة 1: التحقق من الترتيب
        // -------------------------------------------------------
        bool orderCorrect = currentOrder[lastIndex].Trim() == _correctBlockOrder[lastIndex].Trim();

        if (!orderCorrect)
        {
            Debug.Log($"[BlockValidation] ✗ ترتيب خاطئ: ({currentOrder[lastIndex]})");
            EjectLastBlock();

            SendWrongFeedback(
                "order:" + string.Join("|", currentOrder),
                BuildOrderFeedbackLine(lastIndex, currentOrder)
            );
            yield break;
        }

        // -------------------------------------------------------
        // الخطوة 2: التحقق من المحتوى (إن وُجد حقل إدخال)
        // -------------------------------------------------------
        if (aiCompanion != null && aiCompanion.BlockRequiresContentValidation(lastIndex))
        {
            // استخراج القيمة المكتوبة داخل البلوكة
            string enteredValue = GetBlockInputValue(lastIndex);

            bool contentCorrect = aiCompanion.ValidateBlockContent(lastIndex, enteredValue);

            if (!contentCorrect)
            {
                string blockName    = currentOrder[lastIndex];
                string expectedVal  = aiCompanion.GetExpectedValue(lastIndex);
                string inputLabel   = aiCompanion.GetInputLabel(lastIndex);

                Debug.Log($"[BlockValidation] ✗ محتوى خاطئ في ({blockName}) | " +
                          $"{inputLabel}: '{enteredValue}'");

                // البلوكة تبقى في مكانها (لا طرد) لكن تُعاد لحالة "فارغة"
                // حتى يعيد اليوزر الكتابة
                ClearBlockInput(lastIndex);

                SendWrongFeedback(
                    "content:" + blockName + ":" + enteredValue,
                    BuildContentFeedbackLine(inputLabel)
                );
                yield break;
            }

            Debug.Log($"[BlockValidation] ✓ محتوى صحيح في موقع {lastIndex}");
        }

        // -------------------------------------------------------
        // كلا التحققين نجحا
        // -------------------------------------------------------
        Debug.Log($"[BlockValidation] ✓ خطوة صحيحة! ({currentOrder[lastIndex]})");

        // اكتمال التحدي
        if (currentOrder.Count == _correctBlockOrder.Count)
        {
            Debug.Log("[BlockValidation] رائع! اكتمل التحدي بالكامل!");
            SendSuccessFeedback();
            GameEvents.OnChallengeComplete?.Invoke();
        }
    }

    
private List<CodeBlock> GetActiveBlocks()
{
    List<CodeBlock> result = new List<CodeBlock>();

    if (blockDropZone == null)
    {
        Debug.LogWarning("[BlockValidation] blockDropZone غير محدد!");
        return result;
    }

    for (int i = 0; i < blockDropZone.childCount; i++)
    {
        Transform child = blockDropZone.GetChild(i);
        CodeBlock block  = child.GetComponent<CodeBlock>();
        if (block != null && block.gameObject.activeSelf)
            result.Add(block);
    }

    return result;
}

// ===============================================================
// قراءة أسماء البلوكات الحالية
// ===============================================================
private List<string> GetCurrentBlockOrder()
{
    List<string> order = new List<string>();
    foreach (CodeBlock b in GetActiveBlocks())
        order.Add(b.blockName);
    return order;
}

// ===============================================================
// استخراج قيمة الحقل من البلوكة في الموقع المحدد
// ===============================================================
private string GetBlockInputValue(int index)
{
    List<CodeBlock> blocks = GetActiveBlocks();
    if (index < 0 || index >= blocks.Count) return "";
    return blocks[index].GetInputValue();
}

// ===============================================================
// مسح حقل الإدخال داخل البلوكة (دون طردها)
// ===============================================================
private void ClearBlockInput(int index)
{
    List<CodeBlock> blocks = GetActiveBlocks();
    if (index < 0 || index >= blocks.Count) return;
    blocks[index].ClearInputValue();
}

    // ===============================================================
    // طرد آخر بلوكة
    // ===============================================================
    private void EjectLastBlock()
    {
        if (blockDropZone != null && blockDropZone.childCount > 0)
        {
            Transform lastChild = blockDropZone.GetChild(blockDropZone.childCount - 1);
            CodeBlock wrongBlock = lastChild.GetComponent<CodeBlock>();
            if (wrongBlock != null)
                wrongBlock.ResetToOriginalPosition();
        }
    }

    private void SendWrongFeedback(string signature, string instruction)
    {
        _lastFeedbackSignature = signature;

        if (aiCompanion != null)
        {
            AICompanionController.CancelAllPendingVoiceFeedback();

            bool started = aiCompanion.RequestExactVoiceFeedback(
                "",
                instruction,
                false,
                false
            );

            if (started)
            {
                if (!string.IsNullOrEmpty(signature))
                    _playedWrongFeedbackSignatures.Add(signature);

                GameEvents.OnWrongAttempt?.Invoke(aiCompanion.GetAttemptCount());
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(signature))
                _playedWrongFeedbackSignatures.Add(signature);

            GameEvents.OnWrongAttempt?.Invoke(1);
        }
    }

    private void SendSuccessFeedback()
    {
        if (_successFeedbackPlayed || aiCompanion == null)
            return;

        _successFeedbackPlayed = true;
        _lastFeedbackSignature = "";
        _playedWrongFeedbackSignatures.Clear();

        aiCompanion.CancelPendingVoiceFeedback();
        AICompanionController.CancelAllPendingVoiceFeedback();
        aiCompanion.RequestExactVoiceFeedback(
            _storyKey + "_success",
            "أحسنت، أكملت التحدي بالحل الصحيح.",
            true,
            true
        );
    }

    private string BuildOrderFeedbackLine(int index, List<string> currentOrder)
    {
        if (currentOrder == null || currentOrder.Count == 0)
            return "ضع أول خطوة في مكان الحل.";

        if (currentOrder.Count > _correctBlockOrder.Count)
            return "هناك بلوك زائد؛ أبق عدد الخطوات المطلوب فقط.";

        return "الخطوة " + GetArabicStepLabel(index) + " غير مناسبة هنا.";
    }

    private string BuildContentFeedbackLine(string inputLabel)
    {
        if (string.IsNullOrWhiteSpace(inputLabel))
            return "القيمة داخل البلوك غير مناسبة؛ راجعها.";

        return "قيمة " + inputLabel + " غير مناسبة؛ راجعها.";
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
            default: return "الحالية";
        }
    }

    
}
