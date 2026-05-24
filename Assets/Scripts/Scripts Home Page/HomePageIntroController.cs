using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// يتحكم في ظهور المينتور في الصفحة الرئيسية بناءً على اللبس المجهز في المتجر
/// ويضمن عدم بدء الأنيميشن إلا بعد اكتمال تحميل الصورة الصحيحة.
/// </summary>
public class HomePageIntroController : MonoBehaviour
{
    [Header("Mentor/Assistant Elements")]
    public RectTransform assistantTransform; // الكائن المسؤول عن الحركة
    public Image assistantImage;             // الكائن المسؤول عن السبرايت (اللبس)
    public CanvasGroup assistantCanvasGroup;

    [Header("Equipment Data")]
    public ItemData[] allItems;              // اسحبي كل الـ ItemData هنا من الـ Project
    public Sprite defaultBoy;                // صورة الولد الافتراضية
    public Sprite defaultGirl;               // صورة البنت الافتراضية

    [Header("Bubble & Buttons UI")]
    public RectTransform bubbleEmpty;
    public RectTransform bubbleWithText;
    public RectTransform startButton;
    public RectTransform assistantGlow;

    [Header("FX & Sparkles")]
    public RectTransform sparklesRoot;
    public RectTransform[] assistantSparkles;
    public RectTransform[] magicStars;
    public float sparkleRadius = 180f;
    public float sparkleSpinSpeed = 180f;

    [Header("Audio")]
    public AudioSource magicAudio;
    public AudioClip appearClip;
    public HomeAudioController homeAudioController;

    [Header("Animation Settings")]
    public float assistantDelay = 0.2f;
    public float assistantAppearDuration = 1f;
    public float assistantStartScale = 0.15f;
    public float assistantEndScale = 1f;
    public float assistantStartOffsetY = -120f;
    public float spinTurns = 2f;

    [Header("Assistant Idle (Floating)")]
    public float idleFloatAmount = 10f;
    public float idleFloatSpeed = 1.2f;
    public float idleRotateAmount = 2f;
    public float idleScaleAmount = 0.02f;

    // متغيرات داخلية للتحكم
    private CanvasGroup glowCanvasGroup;
    private CanvasGroup bubbleTextCanvasGroup;
    private Vector2 assistantOriginalPosition;
    private Coroutine idleRoutine;
    private bool isDataLoaded = false;

    void Awake()
    {
        // خطوة 1: إخفاء كل العناصر تماماً لمنع ظهور المربع الأبيض قبل التحميل
        if (assistantTransform != null) assistantTransform.gameObject.SetActive(false);
        if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = 0f;

        if (assistantGlow != null) assistantGlow.gameObject.SetActive(false);
        if (bubbleEmpty != null) bubbleEmpty.gameObject.SetActive(false);
        if (bubbleWithText != null) bubbleWithText.gameObject.SetActive(false);
    }

    void Start()
    {
        SetupCanvasGroups();
        // خطوة 2: البدء بجلب البيانات أولاً
        StartCoroutine(LoadUserDataAndInitialize());
    }

    IEnumerator LoadUserDataAndInitialize()
    {
        string currentUserId = PlayerPrefs.GetString("currentUserId", "");

        if (!string.IsNullOrEmpty(currentUserId))
        {
            // جلب البيانات من الفايربيس
            yield return StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) => {

                // جلب الـ ID الخاص باللبس المجهز من الـ PlayerPrefs (نفس لوجك المتجر)
                string lastEquipped = PlayerPrefs.GetString("LastEquipped_" + currentUserId, "");

                // تعيين السبرايت المناسب قبل ظهور المينتور
                ApplyEquipment(user.gender, lastEquipped);
                isDataLoaded = true;
            }, (error) => {
                Debug.LogError("Home Loader: Failed to load user, using defaults.");
                isDataLoaded = true; // نفتح القفل حتى لو فشل لعرض الشكل الافتراضي
            }));
        }
        else
        {
            isDataLoaded = true;
        }

        // الانتظار حتى نضمن أن السبرايت تم تعيينه
        while (!isDataLoaded) yield return null;

        // خطوة 3: الآن الصورة جاهزة، نبدأ تسلسل الأنيميشن
        StartCoroutine(IntroRoutine());
    }

    void ApplyEquipment(string gender, string equippedItemId)
    {
        if (assistantImage == null) return;

        bool itemFound = false;
        // نستخدم حروف صغيرة للمقارنة لضمان الدقة
        string genderLower = gender.ToLower();

        if (!string.IsNullOrEmpty(equippedItemId))
        {
            foreach (var item in allItems)
            {
                if (item.itemId == equippedItemId)
                {
                    assistantImage.sprite = item.characterSprite;
                    itemFound = true;
                    break;
                }
            }
        }

        if (!itemFound)
        {
            bool isGirl = genderLower == "أنثى" || genderLower == "female";
            assistantImage.sprite = isGirl ? defaultGirl : defaultBoy;
        }

        assistantImage.preserveAspect = true;
    }


    IEnumerator IntroRoutine()
    {
        // 1. تجهيز وضعية المساعد (خلف الكواليس)
        if (assistantTransform != null)
        {
            // نضمن أن الشفافية (Alpha) صفر تماماً في البداية لمنع الوميض
            if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = 0f;

            // ضبط الموقع والحجم والزاوية الابتدائية قبل تفعيل الكائن
            assistantTransform.anchoredPosition = assistantOriginalPosition + new Vector2(0, assistantStartOffsetY);
            assistantTransform.localScale = Vector3.one * assistantStartScale;
            assistantTransform.localRotation = Quaternion.identity;

            // تفعيل الكائن (الآن السبرايت المجهز مفترض تم وضعه في ApplyEquipment)
            assistantTransform.gameObject.SetActive(true);
        }

        // 2. تجهيز التوهج (Glow) خلف المساعد
        if (assistantGlow != null)
        {
            assistantGlow.gameObject.SetActive(true);
            assistantGlow.localScale = Vector3.one * 0.2f;
            if (glowCanvasGroup != null) glowCanvasGroup.alpha = 0f;
        }

        // 3. إخفاء العناصر الجانبية لتبدأ بالتسلسل
        if (sparklesRoot != null) sparklesRoot.gameObject.SetActive(false);
        if (bubbleEmpty != null) bubbleEmpty.gameObject.SetActive(false);
        if (bubbleWithText != null) bubbleWithText.gameObject.SetActive(false);
        if (startButton != null) startButton.localScale = Vector3.one * 0.9f;

        // 4. انتظار بسيط (Assistant Delay) قبل بدء "السحر"
        yield return new WaitForSeconds(assistantDelay);

        // 5. التحكم في الصوت والموسيقى
        if (homeAudioController != null) homeAudioController.DuckMusic(0.6f);
        if (magicAudio != null && appearClip != null)
            magicAudio.PlayOneShot(appearClip);

        // 6. تشغيل أنيميشن الظهور السحري (الدوران وتغيير الشفافية)
        // ملاحظة: تأكدي أن هذا الفنكشن لا يحتوي على سطر يغير assistantImage.sprite
        yield return MagicAppearAssistant();

        // 7. تسلسل ظهور فقاعة الكلام الفارغة (Pop Effect)
        yield return new WaitForSeconds(0.4f);
        if (bubbleEmpty != null)
        {
            bubbleEmpty.gameObject.SetActive(true);
            bubbleEmpty.localScale = Vector3.zero;
            yield return Pop(bubbleEmpty, 0f, 1f, 0.35f);
        }

        // 8. ظهور النص داخل الفقاعة (Fade Effect)
        yield return new WaitForSeconds(0.3f);
        if (bubbleWithText != null)
        {
            bubbleWithText.gameObject.SetActive(true);
            if (bubbleTextCanvasGroup != null)
                yield return FadeCanvasGroup(bubbleTextCanvasGroup, 0f, 1f, 0.35f);
        }

        // 9. تفعيل زر البداية مع تأثير النبض المستمر (Pulse)
        yield return new WaitForSeconds(0.3f);
        if (startButton != null)
            StartCoroutine(ButtonPulse());

        // 10. تشغيل دوران النجوم السحرية في الخلفية
        StartCoroutine(StarsLoop());
    }

    IEnumerator MagicAppearAssistant()
    {
        if (assistantTransform == null) yield break;

        float t = 0f;
        float totalRotation = 360f * spinTurns;

        if (sparklesRoot != null) sparklesRoot.gameObject.SetActive(true);

        while (t < assistantAppearDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / assistantAppearDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, p);

            if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = smooth;

            assistantTransform.anchoredPosition = Vector2.Lerp(
                assistantOriginalPosition + new Vector2(0, assistantStartOffsetY),
                assistantOriginalPosition, smooth);

            float bounce = Mathf.Sin(p * Mathf.PI) * 0.08f;
            assistantTransform.localScale = Vector3.one * (Mathf.Lerp(assistantStartScale, assistantEndScale, smooth) + bounce);

            float yRot = Mathf.Lerp(totalRotation, 0f, smooth);
            assistantTransform.localRotation = Quaternion.Euler(0f, yRot, 0f);

            AnimateAssistantSparkles(p);

            if (assistantGlow != null && glowCanvasGroup != null)
            {
                assistantGlow.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.3f, smooth);
                glowCanvasGroup.alpha = Mathf.Sin(p * Mathf.PI) * 0.7f;
            }

            yield return null;
        }

        // تثبيت القيم النهائية
        assistantTransform.anchoredPosition = assistantOriginalPosition;
        assistantTransform.localScale = Vector3.one;
        assistantTransform.localRotation = Quaternion.identity;
        if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = 1f;
        if (glowCanvasGroup != null) glowCanvasGroup.alpha = 0f;
        if (sparklesRoot != null) sparklesRoot.gameObject.SetActive(false);

        idleRoutine = StartCoroutine(AssistantIdleLoop());
    }

    // --- الدوال المساعدة (Idle, Sparkles, Pop, Buttons) ---

    IEnumerator AssistantIdleLoop()
    {
        while (true)
        {
            float wave = Mathf.Sin(Time.time * idleFloatSpeed);
            assistantTransform.anchoredPosition = assistantOriginalPosition + new Vector2(0f, wave * idleFloatAmount);
            assistantTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * idleFloatSpeed * 0.7f) * idleRotateAmount);
            assistantTransform.localScale = Vector3.one * (1f + Mathf.Abs(wave) * idleScaleAmount);
            yield return null;
        }
    }

    void AnimateAssistantSparkles(float progress)
    {
        if (assistantSparkles == null) return;
        float alpha = Mathf.Sin(progress * Mathf.PI);
        for (int i = 0; i < assistantSparkles.Length; i++)
        {
            float angle = (Time.time * sparkleSpinSpeed) + (360f / assistantSparkles.Length) * i;
            float rad = angle * Mathf.Deg2Rad;
            assistantSparkles[i].anchoredPosition = new Vector2(Mathf.Cos(rad) * sparkleRadius, Mathf.Sin(rad) * sparkleRadius);
            assistantSparkles[i].GetComponent<CanvasGroup>().alpha = alpha;
        }
    }

    void SetupCanvasGroups()
    {
        if (assistantTransform != null) assistantOriginalPosition = assistantTransform.anchoredPosition;
        if (assistantCanvasGroup == null && assistantTransform != null) assistantCanvasGroup = assistantTransform.gameObject.AddComponent<CanvasGroup>();
        if (assistantGlow != null && glowCanvasGroup == null) glowCanvasGroup = assistantGlow.gameObject.AddComponent<CanvasGroup>();
        if (bubbleWithText != null && bubbleTextCanvasGroup == null) bubbleTextCanvasGroup = bubbleWithText.gameObject.AddComponent<CanvasGroup>();
    }

    IEnumerator Pop(RectTransform target, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, t / duration);
            target.localScale = Vector3.one * (Mathf.Lerp(from, to, smooth) + (Mathf.Sin(smooth * Mathf.PI) * 0.1f));
            yield return null;
        }
        target.localScale = Vector3.one * to;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator ButtonPulse()
    {
        while (true)
        {
            yield return Pop(startButton, 1f, 1.06f, 0.7f);
            yield return Pop(startButton, 1.06f, 1f, 0.7f);
        }
    }

    IEnumerator StarsLoop()
    {
        while (true)
        {
            foreach (var star in magicStars)
            {
                if (star != null)
                {
                    star.Rotate(0, 0, 20 * Time.deltaTime);
                    star.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 2f) * 0.05f);
                }
            }
            yield return null;
        }
    }
}