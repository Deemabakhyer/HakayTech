using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Controls the visual rendering of the AI Companion on the home page based on the equipped store item.
/// Ensures the introduction sequence initiates only after the correct character sprite is fully loaded.
/// </summary>
public class HomePageIntroController : MonoBehaviour
{
    [Header("Mentor/Assistant Elements")]
    public RectTransform assistantTransform; 
    public Image assistantImage;             
    public CanvasGroup assistantCanvasGroup;

    [Header("Equipment Data")]
    public ItemData[] allItems;              
    public Sprite defaultBoy;               
    public Sprite defaultGirl;               

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
    private CanvasGroup glowCanvasGroup;
    private CanvasGroup bubbleTextCanvasGroup;
    private Vector2 assistantOriginalPosition;
    private Coroutine idleRoutine;
    private bool isDataLoaded = false;

    void Awake()
    {
        if (assistantTransform != null) assistantTransform.gameObject.SetActive(false);
        if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = 0f;

        if (assistantGlow != null) assistantGlow.gameObject.SetActive(false);
        if (bubbleEmpty != null) bubbleEmpty.gameObject.SetActive(false);
        if (bubbleWithText != null) bubbleWithText.gameObject.SetActive(false);
    }

    void Start()
    {
        SetupCanvasGroups();
        StartCoroutine(LoadUserDataAndInitialize());
    }


    IEnumerator LoadUserDataAndInitialize()
    {
        PerformanceLogger.Instance.StartMeasure("HomePage_CharacterLoad");
        string currentUserId = PlayerPrefs.GetString("currentUserId", "");

        if (!string.IsNullOrEmpty(currentUserId))
        {
            yield return StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) => {

                string lastEquipped = PlayerPrefs.GetString("LastEquipped_" + currentUserId, "");
                ApplyEquipment(user.gender, lastEquipped);
                isDataLoaded = true;
            }, (error) => {
                Debug.LogError("Home Loader: Failed to load user, using defaults.");
                isDataLoaded = true; 
            }));
        }
        else
        {
            isDataLoaded = true;
        }
        while (!isDataLoaded) yield return null;
        PerformanceLogger.Instance.StopMeasure("HomePage_CharacterLoad");



        StartCoroutine(IntroRoutine());

    }

    void ApplyEquipment(string gender, string equippedItemId)
    {
        if (assistantImage == null) return;

        bool itemFound = false;
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
            bool isGirl = genderLower == "أنثى" || genderLower == "انثى";
            assistantImage.sprite = isGirl ? defaultGirl : defaultBoy;
        }

        assistantImage.preserveAspect = true;
    }

    IEnumerator IntroRoutine()
    {
        if (assistantTransform != null)
        {
            if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = 0f;

            assistantTransform.anchoredPosition = assistantOriginalPosition + new Vector2(0, assistantStartOffsetY);
            assistantTransform.localScale = Vector3.one * assistantStartScale;
            assistantTransform.localRotation = Quaternion.identity;
            assistantTransform.gameObject.SetActive(true);
        }

        if (assistantGlow != null)
        {
            assistantGlow.gameObject.SetActive(true);
            assistantGlow.localScale = Vector3.one * 0.2f;
            if (glowCanvasGroup != null) glowCanvasGroup.alpha = 0f;
        }

        if (sparklesRoot != null) sparklesRoot.gameObject.SetActive(false);
        if (bubbleEmpty != null) bubbleEmpty.gameObject.SetActive(false);
        if (bubbleWithText != null) bubbleWithText.gameObject.SetActive(false);
        if (startButton != null) startButton.localScale = Vector3.one * 0.9f;

        yield return new WaitForSeconds(assistantDelay);

        if (homeAudioController != null) homeAudioController.DuckMusic(0.6f);
        if (magicAudio != null && appearClip != null)
            magicAudio.PlayOneShot(appearClip);

        yield return MagicAppearAssistant();

        yield return new WaitForSeconds(0.4f);
        if (bubbleEmpty != null)
        {
            bubbleEmpty.gameObject.SetActive(true);
            bubbleEmpty.localScale = Vector3.zero;
            yield return Pop(bubbleEmpty, 0f, 1f, 0.35f);
        }

        yield return new WaitForSeconds(0.3f);
        if (bubbleWithText != null)
        {
            bubbleWithText.gameObject.SetActive(true);
            if (bubbleTextCanvasGroup != null)
                yield return FadeCanvasGroup(bubbleTextCanvasGroup, 0f, 1f, 0.35f);
        }
        yield return new WaitForSeconds(0.3f);
        if (startButton != null)
            StartCoroutine(ButtonPulse());

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
        assistantTransform.anchoredPosition = assistantOriginalPosition;
        assistantTransform.localScale = Vector3.one;
        assistantTransform.localRotation = Quaternion.identity;
        if (assistantCanvasGroup != null) assistantCanvasGroup.alpha = 1f;
        if (glowCanvasGroup != null) glowCanvasGroup.alpha = 0f;
        if (sparklesRoot != null) sparklesRoot.gameObject.SetActive(false);

        idleRoutine = StartCoroutine(AssistantIdleLoop());
    }

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