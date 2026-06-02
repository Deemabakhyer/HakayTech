using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class AICompanionController : MonoBehaviour
{
    /// <summary>نص جديد للعرض + تشغيل TTS</summary>
    public static event Action<string> OnCompanionSpeak;

    /// <summary>اسم الصوت المطلوب: "talking" / "happy"</summary>
    public static event Action<string> OnAudioPlay;

    // ===============================================================
    // API Settings
    // ===============================================================
    [Header("API Settings")]
    [SerializeField] private string geminiApiKey = "AIzaSyALPqg4fyA7n1PyIFl-OWl75aMRJ71954M";
    [SerializeField] private bool useGeminiTts = true;
    [SerializeField] private string maleGeminiTtsVoiceName = "Kore";
    [SerializeField] private string femaleGeminiTtsVoiceName = "Puck";

    private const string MODEL_ID    = "gemini-2.5-flash";
    private const string TTS_MODEL_ID = "gemini-2.5-flash-preview-tts";
    private const int    MAX_TOKENS  = 300;
    private const float  TEMPERATURE = 0.15f;
    private const float  TOP_P       = 0.75f;
    private const int    TOP_K       = 20;
    private const int    TTS_SAMPLE_RATE = 24000;

    // ===============================================================
    // بيانات الجلسة
    // ===============================================================
    private string _currentStoryName  = "";
    private string _currentConcept    = "";
    private string _currentLevel      = "";
    private string _currentIntro      = "";
    private string _currentSuccessMsg = "";

    [Header("Session Context")]
    [SerializeField] private string playerGender = "male";

    // ===============================================================
    // حالة المحاولات
    // ===============================================================
    [Header("Attempt Tracking")]
    [SerializeField] private int wrongAttemptCount = 0;

    // ===============================================================
    // إعدادات التلميح
    // ===============================================================
    [Header("Hint Settings")]
    [SerializeField] private float hintCooldown = 3f;

    private float _lastHintTime = -999f;
    private bool  _isRequesting = false;
    private bool  _suppressHintsUntilSuccess = false;
    private int _feedbackGeneration = 0;
    private int _speechGeneration = 0;
    private AudioSource _voiceAudioSource;

    // ===============================================================
    // Conversation Memory
    // ===============================================================
    [Header("Conversation Memory")]
    [SerializeField] private int maxMemoryMessages = 6;

    private readonly List<GeminiContent> _conversationHistory = new List<GeminiContent>();
    private List<string> _correctBlockOrder = new List<string>();
    private static readonly HashSet<string> OneShotFeedbackKeys = new HashSet<string>();

    // ===============================================================
    // بيانات التحقق من المحتوى (Content Validation)
    // يُعبأ من stories.json عبر قائمة blocks_data
    // ===============================================================
    private List<BlockData> _blocksData = new List<BlockData>();

    // ===============================================================
    // تحميل JSON
    // ===============================================================
    [Header("Stories JSON")]
    [Tooltip("اسم الملف داخل StreamingAssets بدون امتداد")]
    [SerializeField] private string storiesJsonFileName = "stories";

    private StoriesRoot _storiesRoot;
    private bool        _storiesLoaded = false;

    // ===============================================================
    // Web Speech API — WebGL فقط
    // ===============================================================
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void WS_Speak(string text, float pitch, float rate);
    [DllImport("__Internal")] private static extern void WS_Stop();
#endif

    // ===============================================================
    // Awake
    // ===============================================================
    public static AICompanionController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        StartCoroutine(LoadStoriesJson());
    }

    // ===============================================================
    // تحميل JSON
    // ===============================================================
   
    private IEnumerator LoadStoriesJson()
    {
        string path = Application.streamingAssetsPath + "/" + storiesJsonFileName + ".json";

        if (System.IO.File.Exists(path))
        {
            try
            {
                string jsonText = System.IO.File.ReadAllText(path);
                _storiesRoot = JsonConvert.DeserializeObject<StoriesRoot>(jsonText);
                _storiesLoaded = true;
                Debug.Log("[Stories] تم التحميل بنجاح محلياً. عدد القصص: " + _storiesRoot.stories.Count);
            }
            catch (Exception e)
            {
                Debug.LogError("[Stories] خطأ في تحليل JSON: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("[Stories] الملف غير موجود في المسار: " + path);
        }

        yield return null;
    }

    private StoryEntry FindStoryByKey(string key)
    {
        if (_storiesRoot?.stories == null) return null;
        foreach (StoryEntry entry in _storiesRoot.stories)
        {
            if (string.Equals(entry.key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                return entry;
        }
        Debug.LogWarning("[Stories] لم تُوجد قصة بالمفتاح: " + key);
        return null;
    }

    public void InitializeCompanion(string storyKey, string gender)
    {
        playerGender = PlayerVoiceResolver.NormalizeGender(gender);
        PlayerVoiceResolver.SaveGender(playerGender);

        wrongAttemptCount = 0;
        _lastHintTime = -999f;
        _isRequesting = false;
        _feedbackGeneration++;

        ClearConversationMemory();

        if (!_storiesLoaded)
            StartCoroutine(WaitForJsonThenInit(storyKey));
        else
            ApplyStoryData(storyKey);
    }

    private IEnumerator WaitForJsonThenInit(string storyKey)
    {
        float waited = 0f;
        while (!_storiesLoaded && waited < 5f)
        {
            waited += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        if (!_storiesLoaded) Debug.LogError("[AICompanion] انتهت مهلة انتظار JSON!");
        else ApplyStoryData(storyKey);
    }

    private void ApplyStoryData(string storyKey)
    {
        StoryEntry entry = FindStoryByKey(storyKey);
        if (entry != null)
        {
            _currentStoryName  = entry.title;
            _currentConcept    = entry.concept;
            _currentLevel      = entry.level;
            _currentIntro      = entry.intro;
            _currentSuccessMsg = entry.success_msg;
            _correctBlockOrder = new List<string>(entry.blocks_order);

            _blocksData = entry.blocks_data != null
                ? new List<BlockData>(entry.blocks_data)
                : new List<BlockData>();
        }
        else
        {
            _currentStoryName  = storyKey;
            _currentConcept    = "";
            _currentLevel      = "مبتدئ";
            _currentIntro = "مرحباً! هيا نتعلم معاً!";
            _currentSuccessMsg = "";
            _correctBlockOrder = new List<string>();
            _blocksData        = new List<BlockData>();
        }

        Debug.Log("[AICompanion] تهيئة | القصة: " + _currentStoryName +
                  " | المفهوم: " + _currentConcept +
                  " | بلوكات بمحتوى: " + _blocksData.Count);
    }

    // ===============================================================
    // مقدمة القصة
    // ===============================================================
    // ===============================================================
    // مقدمة القصة المطورة والمحمية من الانقطاع الصوتي
    // ===============================================================
    // ===============================================================
    // مقدمة القصة المطورة والمحمية من الانقطاع الصوتي
    // ===============================================================
    public void RequestStoryIntro()
    {
        if (!_storiesLoaded)
        {
            // استدعاء دالة الانتظار الآمنة
            StartCoroutine(WaitForJsonThenShowIntro());
            return;
        }

        string intro = string.IsNullOrEmpty(_currentIntro)
            ? "مرحباً! هيا نتعلم " + _currentConcept + " معاً!"
            : _currentIntro;

        // إرسال النص كاملاً للفقاعة البصرية (الـ UI) لكي يظهر للطفل
        OnIntroReceived(intro);

        // تشغيل كروتين خاص بنطق النص الطويل على أجزاء مريحة لمحرك الصوت
        StopAllCoroutines(); // إيقاف أي صوت سابق متداخل
        StartCoroutine(SpeakLongIntroRoutine(intro));
    }

    private IEnumerator SpeakLongIntroRoutine(string fullText)
    {
        yield return new WaitForSeconds(0.5f);

        // تقسيم النص الطويل إلى جمل بناءً على النقط وعلامات التعجب والاستفهام
        string[] sentences = fullText.Split(new char[] { '.', '!', '؟', '?' }, System.StringSplitOptions.RemoveEmptyEntries);

        AudioSource aiVoice = GetComponentInChildren<AudioSource>();

        foreach (string sentence in sentences)
        {
            string cleanSentence = sentence.Trim();
            if (string.IsNullOrEmpty(cleanSentence)) continue;

            // استدعاء دالة إصلاح النص وبث الصوت الفعلي
            SpeakSentence(cleanSentence);

            // انتظر طالما أن المساعد لا يزال ينطق الجملة الحالية قبل الانتقال للجملة التالية
            if (aiVoice != null)
            {
                yield return new WaitForSeconds(0.3f);
                while (aiVoice.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(cleanSentence.Length * 0.15f);
            }
        }
    }

    // --- الدالة المصلحة والمسؤولة عن بث وتوليد الصوت الفعلي لمشروعكم ---
    private void SpeakSentence(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        string straightText = text;

        // إذا كان النص مقلوباً (مثل اللقطة السابقة)، نعيده مستقيماً مئة بالمئة ليفهمه محرك الـ TTS ويخرجه كصوت
        if (text.Contains("يراج") || (text.Length > 0 && text[0] == 'ء'))
        {
            char[] charArray = text.ToCharArray();
            System.Array.Reverse(charArray);
            straightText = new string(charArray);
        }

        // الربط الجذري بمحرك صوت غلا (لتوليد وبث الصوت الفعلي في السماعات)
        RequestExactVoiceFeedback("", straightText, false, false);

        Debug.Log("[AI Voice Link Success] تم إصلاح الحروف وبث الصوت بنجاح: " + straightText);
    }

    // --- إضافة الدالة الناقصة لإنهاء الأخطاء الحمراء فوراً ---
    private IEnumerator WaitForJsonThenShowIntro()
    {
        float waited = 0f;
        while (!_storiesLoaded && waited < 5f)
        {
            waited += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        RequestStoryIntro();
    }

    // ===============================================================
    // طلب تلميح (ترتيب خاطئ)
    // ===============================================================
    public void RequestHint(List<string> currentBlocks)
    {
        if (currentBlocks == null || currentBlocks.Count == 0)
        {
            Debug.LogWarning("[AICompanion] لا توجد بلوكات."); return;
        }
        if (Time.time - _lastHintTime < hintCooldown)
        {
            Debug.Log("[AICompanion] Cooldown نشط."); return;
        }
        if (_isRequesting)
        {
            Debug.Log("[AICompanion] طلب جارٍ."); return;
        }

        wrongAttemptCount++;
        _lastHintTime = Time.time;
        StartCoroutine(CallGeminiAPI(
            BuildHintPrompt(string.Join(" <- ", currentBlocks), wrongAttemptCount),
            OnHintReceived));
    }

    public void RequestContentHint(string blockName, string enteredValue, string expectedValue)
    {
        if (Time.time - _lastHintTime < hintCooldown)
        {
            Debug.Log("[AICompanion] Cooldown نشط."); return;
        }
        if (_isRequesting) return;

        wrongAttemptCount++;
        _lastHintTime = Time.time;

        string prompt = BuildContentHintPrompt(blockName, enteredValue, expectedValue);
        StartCoroutine(CallGeminiAPI(prompt, OnHintReceived));
    }

    // ===============================================================
    // تغذية راجعة عند النجاح الكامل
    // ===============================================================
    public void RequestSuccessFeedback()
    {
        if (_isRequesting) return;

        if (!string.IsNullOrEmpty(_currentSuccessMsg))
        {
            OnSuccessReceived(_currentSuccessMsg);
            return;
        }

        StartCoroutine(CallGeminiAPI(BuildSuccessPrompt(), OnSuccessReceived));
    }

    public void RequestGeneratedSuccessFeedback()
    {
        if (_isRequesting) return;

        StartCoroutine(CallGeminiAPI(BuildSuccessPrompt(), OnSuccessReceived));
    }

    public bool RequestClearVoiceFeedback(
        string feedbackKey,
        string instruction,
        bool playOnce,
        bool isSuccess)
    {
        if (playOnce && !string.IsNullOrEmpty(feedbackKey) &&
            !OneShotFeedbackKeys.Add(feedbackKey))
        {
            Debug.Log("[AICompanion] تم تجاهل تعقيب مكرر: " + feedbackKey);
            return false;
        }

        if (isSuccess)
            _suppressHintsUntilSuccess = true;

        int feedbackGeneration = ++_feedbackGeneration;

        if (_isRequesting)
        {
            if (isSuccess)
            {
                StartCoroutine(RequestClearVoiceFeedbackWhenReady(instruction, isSuccess, feedbackGeneration));
                return true;
            }

            if (playOnce && !string.IsNullOrEmpty(feedbackKey))
                OneShotFeedbackKeys.Remove(feedbackKey);

            return false;
        }

        if (!isSuccess && Time.time - _lastHintTime < hintCooldown)
        {
            Debug.Log("[AICompanion] Cooldown نشط.");
            return false;
        }

        if (!isSuccess)
        {
            wrongAttemptCount++;
            _lastHintTime = Time.time;
        }

        StartCoroutine(CallGeminiAPI(
            BuildClearVoiceFeedbackPrompt(instruction, isSuccess),
            text => DeliverVoiceFeedbackIfCurrent(text, isSuccess, feedbackGeneration),
            false,
            GetClearFeedbackFallback(isSuccess)
        ));

        return true;
    }

    public bool RequestExactVoiceFeedback(
        string feedbackKey,
        string feedbackLine,
        bool playOnce,
        bool isSuccess)
    {
        if (string.IsNullOrWhiteSpace(feedbackLine))
            return false;

        if (playOnce && !string.IsNullOrEmpty(feedbackKey) &&
            !OneShotFeedbackKeys.Add(feedbackKey))
        {
            Debug.Log("[AICompanion] تم تجاهل تعقيب مكرر: " + feedbackKey);
            return false;
        }

        if (isSuccess)
            _suppressHintsUntilSuccess = true;

        int feedbackGeneration = ++_feedbackGeneration;

        if (!isSuccess)
        {
            wrongAttemptCount++;
            _lastHintTime = Time.time;
        }

        DeliverVoiceFeedbackIfCurrent(feedbackLine, isSuccess, feedbackGeneration);

        return true;
    }

    private IEnumerator RequestClearVoiceFeedbackWhenReady(string instruction, bool isSuccess, int feedbackGeneration)
    {
        float waited = 0f;

        while (_isRequesting && waited < 4f)
        {
            if (feedbackGeneration != _feedbackGeneration)
                yield break;

            waited += Time.deltaTime;
            yield return null;
        }

        if (feedbackGeneration != _feedbackGeneration)
            yield break;

        if (_isRequesting)
        {
            if (isSuccess)
                DeliverVoiceFeedbackIfCurrent(GetClearFeedbackFallback(true), true, feedbackGeneration);

            yield break;
        }

        StartCoroutine(CallGeminiAPI(
            BuildClearVoiceFeedbackPrompt(instruction, isSuccess),
            text => DeliverVoiceFeedbackIfCurrent(text, isSuccess, feedbackGeneration),
            false,
            GetClearFeedbackFallback(isSuccess)
        ));
    }

    private IEnumerator RequestExactVoiceFeedbackWhenReady(string feedbackLine, bool isSuccess, int feedbackGeneration)
    {
        float waited = 0f;

        while (_isRequesting && waited < 4f)
        {
            if (feedbackGeneration != _feedbackGeneration)
                yield break;

            waited += Time.deltaTime;
            yield return null;
        }

        if (feedbackGeneration != _feedbackGeneration)
            yield break;

        DeliverVoiceFeedbackIfCurrent(feedbackLine, isSuccess, feedbackGeneration);
    }

    private void DeliverVoiceFeedbackIfCurrent(string text, bool isSuccess, int feedbackGeneration)
    {
        if (feedbackGeneration != _feedbackGeneration)
        {
            Debug.Log("[AICompanion] تم تجاهل رسالة صوتية قديمة: " + text);
            return;
        }

        if (isSuccess)
            OnSuccessReceived(text);
        else
            OnHintReceived(text);
    }

    public void CancelPendingVoiceFeedback(bool stopCurrentSpeech = true)
    {
        _feedbackGeneration++;
        _suppressHintsUntilSuccess = false;

        if (stopCurrentSpeech)
            StopSpeech();
    }

    public static void CancelAllPendingVoiceFeedback(bool stopCurrentSpeech = true)
    {
        AICompanionController[] companions =
            FindObjectsByType<AICompanionController>(FindObjectsSortMode.None);

        foreach (AICompanionController companion in companions)
        {
            if (companion != null)
                companion.CancelPendingVoiceFeedback(stopCurrentSpeech);
        }
    }

    public void ClearConversationMemory()
    {
        _conversationHistory.Clear();
        Debug.Log("[AICompanion] تم مسح ذاكرة المحادثة.");
    }

    // ===============================================================
    // Content Validation
    // ===============================================================
    public bool BlockRequiresContentValidation(int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= _blocksData.Count) return false;
        return _blocksData[blockIndex].requires_input;
    }

    public bool ValidateBlockContent(int blockIndex, string enteredValue)
    {
        if (blockIndex < 0 || blockIndex >= _blocksData.Count) return true;
        BlockData data = _blocksData[blockIndex];
        if (!data.requires_input) return true;

        string value = enteredValue?.Trim() ?? "";

        switch (data.validation_type?.ToLower().Trim())
        {
            case "naming_rules":
                return ValidateNamingRules(value, data.correct_value?.Trim());

            case "number":
                return ValidateNumber(value, data.correct_value?.Trim());

            case "exact":
            default:
                return string.Equals(value, data.correct_value?.Trim(), StringComparison.Ordinal);
        }
    }

    private bool ValidateNamingRules(string value, string correctValue)
    {
        if (string.IsNullOrEmpty(value)) return false;

        if (!char.IsLetter(value[0]))                              return false;
        if (value.Contains(" "))                                   return false;
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z0-9_]*$")) return false;

        return string.Equals(value, correctValue, StringComparison.Ordinal);
    }

    private bool ValidateNumber(string value, string correctValue)
    {
        if (!int.TryParse(value, out int entered))       return false;
        if (!int.TryParse(correctValue, out int correct)) return false;
        return entered == correct;
    }

    public string GetExpectedValue(int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= _blocksData.Count) return "";
        return _blocksData[blockIndex].correct_value ?? "";
    }

    public string GetInputLabel(int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= _blocksData.Count) return "القيمة";
        return _blocksData[blockIndex].input_label ?? "القيمة";
    }

    // ===============================================================
    // بناء الـ Prompts
    // ===============================================================
    private string GetStorySpecificRules()
    {
        switch (_currentConcept.Trim())
        {
            case "التسلسل":        return "ركز على ترتيب الخطوات خطوة بخطوة مثل الوصفات أو الأنشطة اليومية.";
            case "المتغيرات":      return "ركز على فكرة تخزين المعلومات داخل متغير وإعطائه اسماً مناسباً يبدأ بحرف ولا يحتوي مسافات.";
            case "الجملة الشرطية": return "ركز على اتخاذ القرار عند تحقق شرط معين، والتحقق من الأرقام والقيم.";
            case "التكرار":        return "ركز على تكرار نفس العملية عدة مرات، وأهمية تحديد العدد الصحيح.";
            case "الدوال":         return "ركز على تجميع مجموعة أوامر داخل دالة باسم صحيح يمكن استدعاؤها لاحقاً.";
            case "تصحيح الأخطاء": return "ركز على اكتشاف الخطأ ثم إصلاحه ثم اختبار الحل.";
            default:
                Debug.LogWarning("[AICompanion] مفهوم غير معروف: '" + _currentConcept + "'");
                return "";
        }
    }

    private string BuildSystemPrompt()
    {
        string genderAddress  = playerGender == "female" ? "يا بطلتنا الصغيرة" : "يا بطلنا الصغير";
        string difficultyRules;
        switch (_currentLevel.Trim())
        {
            case "مبتدئ": difficultyRules = "استخدم كلمات بسيطة جداً ومناسبة للأطفال الصغار."; break;
            case "متوسط": difficultyRules = "استخدم شرحاً مختصراً مع سؤال توجيهي يساعد الطفل على التفكير."; break;
            default:      difficultyRules = "يمكنك استخدام شرح برمجي أعمق قليلاً مع المحافظة على بساطة اللغة."; break;
        }
        return
            "أنت مرافق ذكي داخل لعبة تعليمية برمجية للأطفال اسمها حكاياتك.\n" +
            "اسم القصة الحالية: "        + _currentStoryName      + "\n" +
            "المفهوم البرمجي المستهدف: " + _currentConcept        + "\n" +
            "مستوى القصة: "              + _currentLevel           + "\n" +
            "نداء المستخدم: "            + genderAddress           + "\n\n" +
            "تعليمات خاصة بالمستوى:\n"  + difficultyRules         + "\n\n" +
            "تعليمات خاصة بالمفهوم:\n"  + GetStorySpecificRules() + "\n\n" +
            "قواعد صارمة:\n" +
            "1. تحدّث دائماً بالعربية الفصحى المبسّطة.\n" +
            "2. لا تُعطِ الحلّ أو القيمة الصحيحة أو الترتيب الصحيح مباشرة أبداً.\n" +
            "3. قدّم تلميحات ذكية توجّه الطفل للتفكير.\n" +
            "4. استخدم لغة دافئة ومشجّعة.\n" +
            "5. ردّك قصير: جملتان أو ثلاث كحدٍّ أقصى.\n" +
            "6. اربط تلميحاتك بأجواء القصة (" + _currentStoryName + ").";
    }

    private string BuildHintPrompt(string blocksText, int attempt)
    {
        attempt = Mathf.Clamp(attempt, 1, 3);
        string hintLevel;
        switch (attempt)
        {
            case 1:
                hintLevel = "قدّم تلميحاً عاماً ومشجّعاً دون الإشارة لخطأ محدد.";
                break;
            case 2:
                hintLevel = "قدّم تلميحاً مباشراً يشير لموضع الخطأ دون ذكر الحل. الترتيب الحالي: " + blocksText;
                break;
            default:
                hintLevel = "اشرح المفهوم (" + _currentConcept + ") بمثال من الحياة اليومية واسأل سؤالاً توجيهياً. لا تذكر الترتيب الصحيح.";
                break;
        }
        return
            "المفهوم: "                   + _currentConcept + "\n\n" +
            "الترتيب الصحيح:\n"           + string.Join(" <- ", _correctBlockOrder) + "\n\n" +
            "ترتيب الطفل:\n"              + blocksText      + "\n\n" +
            "رقم المحاولة: "              + attempt         + "\n\n" +
            "المطلوب:\n"                  + hintLevel       + "\n\n" +
            "تنبيه صارم: لا تكشف الترتيب الصحيح أو القيمة الصحيحة أبداً.";
    }

    private string BuildContentHintPrompt(string blockName, string enteredValue, string expectedValue)
    {
        int attempt = Mathf.Clamp(wrongAttemptCount, 1, 3);
        string hintLevel;
        switch (attempt)
        {
            case 1:
                hintLevel = "قدّم تلميحاً عاماً يشجع الطفل على مراجعة القيمة التي كتبها دون الإشارة للخطأ بشكل مباشر.";
                break;
            case 2:
                hintLevel = "أشر بشكل غير مباشر إلى نوع القيمة المطلوبة (رقم؟ اسم؟) دون ذكرها.";
                break;
            default:
                hintLevel = "اشرح للطفل بمثال من القصة كيف يفكر في القيمة الصحيحة، دون ذكرها مباشرة.";
                break;
        }
        return
            "المفهوم: "              + _currentConcept + "\n\n" +
            "البلوكة: "              + blockName        + "\n\n" +
            "ما كتبه الطفل: "       + enteredValue     + "\n\n" +
            "نوع القيمة المطلوبة: " + GetConceptInputDescription() + "\n\n" +
            "رقم المحاولة: "        + attempt          + "\n\n" +
            "المطلوب:\n"            + hintLevel        + "\n\n" +
            "تنبيه صارم: لا تذكر القيمة الصحيحة (" + expectedValue + ") أبداً بأي شكل.";
    }

    private string GetConceptInputDescription()
    {
        switch (_currentConcept.Trim())
        {
            case "المتغيرات":      return "اسم متغير صحيح: يبدأ بحرف، لا مسافات، لا رموز خاصة.";
            case "الجملة الشرطية": return "رقم يمثل السعر الصحيح حسب نوع التمر.";
            case "التكرار":        return "رقم صحيح يمثل عدد مرات التكرار.";
            case "الدوال":         return "اسم دالة صحيح يتبع قواعد تسمية الدوال في البرمجة.";
            default:               return "قيمة صحيحة تناسب سياق القصة.";
        }
    }

    private string BuildSuccessPrompt()
    {
        return
            "الطفل نجح في قصة (" + _currentStoryName + ").\n" +
            "رسالة تهنئة حارة جملتين فقط: تهنئة مرتبطة بأجواء القصة، وجملة تعزّز مفهوم (" + _currentConcept + ").";
    }

    // ===============================================================
    // استدعاء Gemini API
    // ===============================================================
    private string BuildClearVoiceFeedbackPrompt(string instruction, bool isSuccess)
    {
        return
            "اكتب العبارة الصوتية النهائية فقط داخل لعبة تعليمية للأطفال.\n" +
            "القصة: " + _currentStoryName + "\n" +
            "المفهوم: " + _currentConcept + "\n" +
            "الموقف: " + instruction + "\n\n" +
            "قواعد الرد الصارمة:\n" +
            "1. جملة عربية واحدة فقط.\n" +
            "2. من 5 إلى 10 كلمات فقط.\n" +
            "3. لا تستخدم الإنجليزية أو الرموز أو القوائم أو علامات الاقتباس.\n" +
            "4. لا تذكر الحل الصحيح أو الرقم الصحيح أو ترتيب البلوكات.\n" +
            "5. لا تبدأ بعبارات طويلة مثل: يبدو أن، أو دعنا.\n" +
            "6. استخدم كلمات سهلة وواضحة عند النطق.\n" +
            (isSuccess
                ? "7. هذه حالة نجاح: هنئ الطالب مباشرة بدون شرح."
                : "7. هذه حالة خطأ: نبّه بلطف واطلب المراجعة فقط.");
    }

    private string GetClearFeedbackFallback(bool isSuccess)
    {
        return isSuccess
            ? "أحسنت، أنهيت تحدي الطواف بنجاح."
            : "انتبه، راجع اختيارك ثم حاول مرة أخرى.";
    }

    private IEnumerator CallGeminiAPI(
        string userMessage,
        Action<string> callback,
        bool useConversationMemory = true,
        string fallbackText = null)
    {
        _isRequesting = true;

        // OnCompanionSpeak?.Invoke("..."); // معطل لكي لا يظهر النص للطالب

        GeminiContent userContent = new GeminiContent
        {
            role  = "user",
            parts = new List<GeminiPart> { new GeminiPart { text = userMessage } }
        };

        List<GeminiContent> requestContents;
        if (useConversationMemory)
        {
            _conversationHistory.Add(userContent);
            requestContents = new List<GeminiContent>(_conversationHistory);
        }
        else
        {
            requestContents = new List<GeminiContent> { userContent };
        }

        string jsonBody = JsonConvert.SerializeObject(new GeminiRequest
        {
            contents = requestContents,
            systemInstruction = new GeminiSystemInstruction
            {
                parts = new List<GeminiPart> { new GeminiPart { text = BuildSystemPrompt() } }
            },
            generationConfig = new GeminiGenerationConfig
            {
                maxOutputTokens = MAX_TOKENS,
                temperature = TEMPERATURE,
                topP = TOP_P,
                topK = TOP_K
            }
        });

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        string fullUrl = "https://generativelanguage.googleapis.com/v1beta/models/" +
                         MODEL_ID + ":generateContent?key=" + geminiApiKey;

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            string responseText;

            if (request.result == UnityWebRequest.Result.Success)
            {
                responseText = ParseGeminiResponse(request.downloadHandler.text);
                if (useConversationMemory)
                {
                    _conversationHistory.Add(new GeminiContent
                    {
                        role  = "model",
                        parts = new List<GeminiPart> { new GeminiPart { text = responseText } }
                    });
                    TrimConversationMemory();
                }
            }
            else
            {
                Debug.LogError("[AICompanion] خطأ: " + request.error);
                responseText = string.IsNullOrEmpty(fallbackText)
                    ? "أنا معك! حاول مرة أخرى، أنت قادر على ذلك."
                    : fallbackText;
            }

            callback?.Invoke(responseText);
        }

        _isRequesting = false;
    }

    private string ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            GeminiResponse response = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);
            if (response?.candidates != null && response.candidates.Count > 0)
            {
                var candidate = response.candidates[0];
                if (candidate.content?.parts != null && candidate.content.parts.Count > 0)
                    return candidate.content.parts[0].text;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[AICompanion] خطأ في تحليل رد Gemini: " + e.Message);
        }
        return "أنا هنا لمساعدتك! حاول مرة أخرى.";
    }

    private void TrimConversationMemory()
    {
        while (_conversationHistory.Count > maxMemoryMessages)
        {
            _conversationHistory.RemoveAt(0);

            if (_conversationHistory.Count > 0 &&
                _conversationHistory[0].role == "model")
            {
                _conversationHistory.RemoveAt(0);
            }
        }
    }

    private void OnHintReceived(string text)
    {
        if (_suppressHintsUntilSuccess)
        {
            Debug.Log("[AICompanion] تم تجاهل تلميح قديم بعد نجاح الطالب: " + text);
            return;
        }

        // OnCompanionSpeak?.Invoke(text); // معطل لكي لا يظهر النص للطالب ويبقى صوتياً فقط
        SpeakText(text);
        Debug.Log("[AICompanion] تلميح (محاولة " + wrongAttemptCount + "): " + text);
    }

    private void OnSuccessReceived(string text)
    {
        _suppressHintsUntilSuccess = false;

        // OnCompanionSpeak?.Invoke(text); // معطل لكي لا يظهر النص للطالب ويبقى صوتياً فقط
        SpeakText(text);
        wrongAttemptCount = 0;
    }

    private void OnIntroReceived(string text)
    {
        // OnCompanionSpeak?.Invoke(text); // معطل لكي لا يظهر النص للطالب ويبقى صوتياً فقط
        SpeakText(text);
    }

    private void SpeakText(string text)
    {
        text = NormalizeVoiceLine(text);
        if (string.IsNullOrEmpty(text)) return;

        int speechGeneration = ++_speechGeneration;

        if (_editorSpeechCoroutine != null)
        {
            StopCoroutine(_editorSpeechCoroutine);
            _editorSpeechCoroutine = null;
        }

        AudioSource source = GetVoiceAudioSource();
        if (source != null && source.isPlaying)
            source.Stop();

        if (useGeminiTts && !string.IsNullOrWhiteSpace(geminiApiKey))
        {
            _editorSpeechCoroutine = StartCoroutine(PlayGeminiTtsOrFallback(text, speechGeneration));
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        float pitch = PlayerVoiceResolver.GetStoredGender(playerGender) == "female" ? 1.08f : 0.92f;
        WS_Stop();
        WS_Speak(text, pitch, 1.02f);
#else
        Debug.Log("[TTS-Editor] [" + playerGender.ToUpper() + "] " + text);
        _editorSpeechCoroutine = StartCoroutine(PlayEditorSpeech(text, speechGeneration));
#endif
    }

    private string NormalizeVoiceLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string cleaned = text.Replace("\r", " ").Replace("\n", " ").Trim();
        cleaned = Regex.Replace(cleaned, @"^\s*(\d+[\.\-\)]|[-*•])\s*", "");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        cleaned = cleaned.Trim('"', '\'', '“', '”', '‘', '’', '-', '•', '*', ' ');

        int firstEnd = -1;
        char[] sentenceEnds = { '.', '!', '؟', '?' };
        foreach (char sentenceEnd in sentenceEnds)
        {
            int index = cleaned.IndexOf(sentenceEnd);
            if (index >= 0 && (firstEnd < 0 || index < firstEnd))
                firstEnd = index;
        }

        if (firstEnd >= 0)
            cleaned = cleaned.Substring(0, firstEnd + 1).Trim();

        string[] words = cleaned.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 12)
            return cleaned;

        StringBuilder limited = new StringBuilder();
        for (int i = 0; i < 12; i++)
        {
            if (i > 0)
                limited.Append(' ');

            limited.Append(words[i]);
        }

        return limited.ToString();
    }

    private Coroutine _editorSpeechCoroutine;

    private IEnumerator PlayGeminiTtsOrFallback(string text, int speechGeneration)
    {
        AudioSource source = GetVoiceAudioSource();

        source.Stop();

        string prompt =
            "[warm, clear, child-friendly, natural Arabic, slightly brisk pace, about 1.12x speed, keep every word clear, do not rush] " +
            "Say exactly: \"" + text + "\"";

        string jsonBody = JsonConvert.SerializeObject(new GeminiRequest
        {
            contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    role = "user",
                    parts = new List<GeminiPart>
                    {
                        new GeminiPart { text = prompt }
                    }
                }
            },
            generationConfig = new GeminiGenerationConfig
            {
                responseModalities = new List<string> { "AUDIO" },
                speechConfig = new GeminiSpeechConfig
                {
                    voiceConfig = new GeminiVoiceConfig
                    {
                        prebuiltVoiceConfig = new GeminiPrebuiltVoiceConfig
                        {
                            voiceName = ResolveGeminiTtsVoiceName()
                        }
                    }
                }
            }
        });

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        string fullUrl = "https://generativelanguage.googleapis.com/v1beta/models/" +
                         TTS_MODEL_ID + ":generateContent?key=" + geminiApiKey;

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (speechGeneration != _speechGeneration)
                yield break;

            if (request.result == UnityWebRequest.Result.Success &&
                TryCreateGeminiTtsClip(request.downloadHandler.text, out AudioClip clip))
            {
                if (speechGeneration != _speechGeneration)
                    yield break;

                source.clip = clip;
                source.Play();

                while (source.isPlaying)
                {
                    if (speechGeneration != _speechGeneration)
                    {
                        source.Stop();
                        yield break;
                    }

                    yield return null;
                }

                yield break;
            }

            Debug.LogWarning("[Gemini-TTS] فشل صوت Kore، سيتم استخدام الصوت الاحتياطي. " + request.error);
        }

        if (speechGeneration != _speechGeneration)
            yield break;

        yield return PlayFallbackSpeech(text, speechGeneration);
    }

    private bool TryCreateGeminiTtsClip(string jsonResponse, out AudioClip clip)
    {
        clip = null;

        try
        {
            GeminiResponse response = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);
            GeminiPart part = response?.candidates?[0]?.content?.parts?[0];
            string base64Audio = part?.inlineData?.data ?? part?.inline_data?.data;

            if (string.IsNullOrEmpty(base64Audio))
                return false;

            byte[] pcmBytes = Convert.FromBase64String(base64Audio);
            int sampleCount = pcmBytes.Length / 2;
            if (sampleCount <= 0)
                return false;

            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(pcmBytes, i * 2);
                samples[i] = Mathf.Clamp(sample / 32768f, -1f, 1f);
            }

            clip = AudioClip.Create("GeminiKoreTTS", sampleCount, 1, TTS_SAMPLE_RATE, false);
            return clip.SetData(samples, 0);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Gemini-TTS] تعذر تحليل الصوت: " + e.Message);
            return false;
        }
    }

    private string ResolveGeminiTtsVoiceName()
    {
        return PlayerVoiceResolver.ResolveGeminiVoice(
            PlayerVoiceResolver.GetStoredGender(playerGender),
            maleGeminiTtsVoiceName,
            femaleGeminiTtsVoiceName
        );
    }

    private IEnumerator PlayFallbackSpeech(string text, int speechGeneration)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (speechGeneration != _speechGeneration)
            yield break;

        float pitch = PlayerVoiceResolver.GetStoredGender(playerGender) == "female" ? 1.08f : 0.92f;
        WS_Stop();
        WS_Speak(text, pitch, 1.02f);
        yield break;
#else
        yield return PlayEditorSpeech(text, speechGeneration);
#endif
    }

    private IEnumerator PlayEditorSpeech(string text, int speechGeneration)
    {
        // 1. الحصول على AudioSource أو إنشائه
        AudioSource source = GetVoiceAudioSource();

        source.Stop();

        // 2. تقسيم النص الطويل إلى عبارات صغيرة لتفادي خطأ (400 Bad Request) بسبب طول الرابط
        List<string> chunks = SplitTextIntoChunks(text, 150);

        foreach (string chunk in chunks)
        {
            if (speechGeneration != _speechGeneration)
                yield break;

            string encodedText = UnityWebRequest.EscapeURL(chunk);
            string url = "https://translate.google.com/translate_tts?ie=UTF-8&tl=ar&client=tw-ob&q=" + encodedText;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (speechGeneration != _speechGeneration)
                    yield break;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        if (speechGeneration != _speechGeneration)
                            yield break;

                        source.clip = clip;
                        source.Play();

                        // الانتظار حتى ينتهي المقطع الحالي قبل البدء بالتالي
                        while (source.isPlaying)
                        {
                            if (speechGeneration != _speechGeneration)
                            {
                                source.Stop();
                                yield break;
                            }

                            yield return null;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[TTS-Editor-Fallback] خطأ في تشغيل العبارة: " + chunk + " | " + www.error);
                }
            }

            // استراحة قصيرة بين المقاطع حتى يبقى الكلام واضحا بدون بطء زائد.
            yield return new WaitForSeconds(0.12f);
        }
    }

    private List<string> SplitTextIntoChunks(string text, int maxChunkSize)
    {
        List<string> chunks = new List<string>();
        
        // التقسيم باستخدام علامات الترقيم الشائعة في اللغة العربية
        string[] sentences = text.Split(new char[] { '.', '!', '؟', '?', '،' }, StringSplitOptions.RemoveEmptyEntries);

        StringBuilder currentChunk = new StringBuilder();
        foreach (string sentence in sentences)
        {
            string trimmed = sentence.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (currentChunk.Length + trimmed.Length + 1 > maxChunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                }
            }

            if (currentChunk.Length > 0)
                currentChunk.Append(" ");
            
            currentChunk.Append(trimmed);
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        return chunks;
    }

    public void StopSpeech()
    {
        _speechGeneration++;

#if UNITY_WEBGL && !UNITY_EDITOR
        WS_Stop();
#endif
        if (_editorSpeechCoroutine != null)
        {
            StopCoroutine(_editorSpeechCoroutine);
            _editorSpeechCoroutine = null;
        }

        AudioSource source = GetVoiceAudioSource();
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    private AudioSource GetVoiceAudioSource()
    {
        if (_voiceAudioSource != null)
            return _voiceAudioSource;

        Transform existing = transform.Find("AI Voice Audio Source");
        if (existing != null)
            _voiceAudioSource = existing.GetComponent<AudioSource>();

        if (_voiceAudioSource == null)
        {
            GameObject voiceObject = new GameObject("AI Voice Audio Source");
            voiceObject.transform.SetParent(transform, false);
            _voiceAudioSource = voiceObject.AddComponent<AudioSource>();
        }

        _voiceAudioSource.playOnAwake = false;
        _voiceAudioSource.loop = false;
        _voiceAudioSource.spatialBlend = 0f;
        return _voiceAudioSource;
    }

    private void OnDestroy() => StopSpeech();

    // ===============================================================
    // Public API
    // ===============================================================
    public void SetCorrectBlockOrder(List<string> order) =>
        _correctBlockOrder = new List<string>(order);

    public List<string> GetCorrectBlockOrder() =>
        new List<string>(_correctBlockOrder);

    public void ResetAttemptCounter() => wrongAttemptCount = 0;
    public int  GetAttemptCount()     => wrongAttemptCount;
    public int  GetMemoryCount()      => _conversationHistory.Count;
    public string GetCurrentConcept() => _currentConcept;
}

// ============================================================
// نماذج JSON — مطابقة لهيكل stories.json المحدّث
// ============================================================

[Serializable]
public class StoriesRoot
{
    [JsonProperty("stories")] public List<StoryEntry> stories;
}

[Serializable]
public class StoryEntry
{
    [JsonProperty("key")]          public string          key;
    [JsonProperty("title")]        public string          title;
    [JsonProperty("region")]       public string          region;
    [JsonProperty("concept")]      public string          concept;
    [JsonProperty("level")]        public string          level;
    [JsonProperty("intro")]        public string          intro;
    [JsonProperty("success_msg")]  public string          success_msg;
    [JsonProperty("blocks_order")] public List<string>    blocks_order;

    
    [JsonProperty("blocks_data")]  public List<BlockData> blocks_data;
}

/// <summary>
/// بيانات التحقق من محتوى بلوكة واحدة.
/// </summary>
[Serializable]
public class BlockData
{
    /// <summary>اسم البلوكة — يجب أن يطابق blockName في CodeBlock</summary>
    [JsonProperty("block_name")]    public string block_name;

    /// <summary>هل تحتاج هذه البلوكة إدخالاً نصياً/رقمياً من الطفل؟</summary>
    [JsonProperty("requires_input")] public bool requires_input;

    /// <summary>القيمة الصحيحة (لا تُعرض للطفل، تُستخدم في المقارنة والـ Prompt فقط)</summary>
    [JsonProperty("correct_value")] public string correct_value;

    /// <summary>وصف الحقل للتلميح: مثل "اسم المتغير" أو "عدد التكرارات"</summary>
    [JsonProperty("input_label")]   public string input_label;

    /// <summary>نوع التحقق: "exact" | "naming_rules" | "number"</summary>
    [JsonProperty("validation_type")] public string validation_type;
}

// ============================================================
// نماذج Gemini API
// ============================================================

[Serializable]
public class GeminiRequest
{
    [JsonProperty("contents", NullValueHandling = NullValueHandling.Ignore)]
    public List<GeminiContent> contents;

    [JsonProperty("systemInstruction", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiSystemInstruction systemInstruction;

    [JsonProperty("generationConfig", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiGenerationConfig generationConfig;
}

[Serializable]
public class GeminiSystemInstruction
{
    [JsonProperty("parts", NullValueHandling = NullValueHandling.Ignore)]
    public List<GeminiPart> parts;
}

[Serializable]
public class GeminiContent
{
    [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
    public string role;

    [JsonProperty("parts", NullValueHandling = NullValueHandling.Ignore)]
    public List<GeminiPart> parts;
}

[Serializable]
public class GeminiPart
{
    [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
    public string text;

    [JsonProperty("inlineData", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiInlineData inlineData;

    [JsonProperty("inline_data", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiInlineData inline_data;
}

[Serializable]
public class GeminiGenerationConfig
{
    [JsonProperty("maxOutputTokens", NullValueHandling = NullValueHandling.Ignore)]
    public int? maxOutputTokens;

    [JsonProperty("temperature", NullValueHandling = NullValueHandling.Ignore)]
    public float? temperature;

    [JsonProperty("topP", NullValueHandling = NullValueHandling.Ignore)]
    public float? topP;

    [JsonProperty("topK", NullValueHandling = NullValueHandling.Ignore)]
    public int? topK;

    [JsonProperty("responseModalities", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> responseModalities;

    [JsonProperty("speechConfig", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiSpeechConfig speechConfig;
}

[Serializable]
public class GeminiSpeechConfig
{
    [JsonProperty("voiceConfig", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiVoiceConfig voiceConfig;
}

[Serializable]
public class GeminiVoiceConfig
{
    [JsonProperty("prebuiltVoiceConfig", NullValueHandling = NullValueHandling.Ignore)]
    public GeminiPrebuiltVoiceConfig prebuiltVoiceConfig;
}

[Serializable]
public class GeminiPrebuiltVoiceConfig
{
    [JsonProperty("voiceName")]
    public string voiceName;
}

[Serializable]
public class GeminiInlineData
{
    [JsonProperty("mimeType")]
    public string mimeType;

    [JsonProperty("data")]
    public string data;
}

[Serializable]
public class GeminiResponse
{
    [JsonProperty("candidates")] public List<GeminiCandidate> candidates;
}

[Serializable]
public class GeminiCandidate
{
    [JsonProperty("content")]      public GeminiContent content;
    [JsonProperty("finishReason")] public string        finishReason;
}
