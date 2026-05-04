using System.Collections;
using UnityEngine;

public class HomePageIntroController : MonoBehaviour
{
    [Header("Main Elements")]
    public RectTransform assistant;
    public RectTransform bubbleEmpty;
    public RectTransform bubbleWithText;
    public RectTransform startButton;

    [Header("Optional Magic Glow")]
    public RectTransform assistantGlow;

    [Header("Assistant Sparkles")]
    public RectTransform sparklesRoot;
    public RectTransform[] assistantSparkles;
    public float sparkleRadius = 180f;
    public float sparkleSpinSpeed = 180f;

    [Header("Stars FX")]
    public RectTransform[] magicStars;

    [Header("Audio")]
    public AudioSource magicAudio;
    public AudioClip appearClip;

    [Header("Audio Control")]
    public HomeAudioController homeAudioController;

    [Header("Timing")]
    public float assistantDelay = 0.2f;
    public float bubbleDelay = 0.4f;
    public float bubbleTextDelay = 0.3f;
    public float startButtonDelay = 0.3f;

    [Header("Assistant Magic Appear")]
    public float assistantAppearDuration = 1f;
    public float assistantStartScale = 0.15f;
    public float assistantEndScale = 1f;
    public float assistantStartOffsetY = -120f;
    public float spinTurns = 2f;

    [Header("Assistant Idle")]
    public float idleFloatAmount = 10f;
    public float idleFloatSpeed = 1.2f;
    public float idleRotateAmount = 2f;
    public float idleScaleAmount = 0.02f;

    private CanvasGroup assistantCanvasGroup;
    private CanvasGroup glowCanvasGroup;
    private CanvasGroup bubbleTextCanvasGroup;
    private Vector2 assistantOriginalPosition;
    private Coroutine idleRoutine;

    void Start()
    {
        SetupCanvasGroups();
        StartCoroutine(IntroRoutine());
    }

    void SetupCanvasGroups()
    {
        if (assistant != null)
        {
            assistantOriginalPosition = assistant.anchoredPosition;

            assistantCanvasGroup = assistant.GetComponent<CanvasGroup>();
            if (assistantCanvasGroup == null)
                assistantCanvasGroup = assistant.gameObject.AddComponent<CanvasGroup>();
        }

        if (assistantGlow != null)
        {
            glowCanvasGroup = assistantGlow.GetComponent<CanvasGroup>();
            if (glowCanvasGroup == null)
                glowCanvasGroup = assistantGlow.gameObject.AddComponent<CanvasGroup>();
        }

        if (bubbleWithText != null)
        {
            bubbleTextCanvasGroup = bubbleWithText.GetComponent<CanvasGroup>();
            if (bubbleTextCanvasGroup == null)
                bubbleTextCanvasGroup = bubbleWithText.gameObject.AddComponent<CanvasGroup>();
        }

        if (assistantSparkles != null)
        {
            foreach (RectTransform sparkle in assistantSparkles)
            {
                if (sparkle == null) continue;

                CanvasGroup cg = sparkle.GetComponent<CanvasGroup>();
                if (cg == null)
                    sparkle.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    IEnumerator IntroRoutine()
    {
        if (assistant != null)
        {
            assistant.gameObject.SetActive(true);
            assistant.anchoredPosition =
                assistantOriginalPosition + new Vector2(0, assistantStartOffsetY);

            assistant.localScale = Vector3.one * assistantStartScale;
            assistant.localRotation = Quaternion.identity;

            if (assistantCanvasGroup != null)
                assistantCanvasGroup.alpha = 0f;
        }

        if (assistantGlow != null)
        {
            assistantGlow.gameObject.SetActive(true);
            assistantGlow.localScale = Vector3.one * 0.2f;

            if (glowCanvasGroup != null)
                glowCanvasGroup.alpha = 0f;
        }

        if (sparklesRoot != null)
            sparklesRoot.gameObject.SetActive(false);

        SetSparklesAlpha(0f);

        if (bubbleEmpty != null)
            bubbleEmpty.gameObject.SetActive(false);

        if (bubbleWithText != null)
        {
            bubbleWithText.gameObject.SetActive(false);
            bubbleWithText.localScale = Vector3.one;

            if (bubbleTextCanvasGroup != null)
                bubbleTextCanvasGroup.alpha = 0f;
        }

        if (startButton != null)
            startButton.localScale = Vector3.one * 0.9f;

        yield return new WaitForSeconds(assistantDelay);

        if (homeAudioController != null)
            homeAudioController.DuckMusic(0.6f);

        if (magicAudio != null && appearClip != null)
            magicAudio.PlayOneShot(appearClip);

        yield return MagicAppearAssistant();

        yield return new WaitForSeconds(bubbleDelay);

        if (bubbleEmpty != null)
        {
            bubbleEmpty.gameObject.SetActive(true);
            bubbleEmpty.localScale = Vector3.zero;
            yield return Pop(bubbleEmpty, 0f, 1f, 0.35f);
        }

        yield return new WaitForSeconds(bubbleTextDelay);

        if (bubbleWithText != null)
        {
            bubbleWithText.gameObject.SetActive(true);

            if (bubbleTextCanvasGroup != null)
                yield return FadeCanvasGroup(bubbleTextCanvasGroup, 0f, 1f, 0.35f);
        }

        yield return new WaitForSeconds(startButtonDelay);

        if (startButton != null)
            StartCoroutine(ButtonPulse());

        StartCoroutine(StarsLoop());
    }

    IEnumerator MagicAppearAssistant()
    {
        if (assistant == null)
            yield break;

        float t = 0f;
        float totalRotation = 360f * spinTurns;

        if (sparklesRoot != null)
        {
            sparklesRoot.gameObject.SetActive(true);
            sparklesRoot.anchoredPosition = assistantOriginalPosition;
            sparklesRoot.localScale = Vector3.one;
        }

        SetSparklesAlpha(0f);

        while (t < assistantAppearDuration)
        {
            t += Time.deltaTime;

            float p = Mathf.Clamp01(t / assistantAppearDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, p);

            if (assistantCanvasGroup != null)
                assistantCanvasGroup.alpha = smooth;

            assistant.anchoredPosition = Vector2.Lerp(
                assistantOriginalPosition + new Vector2(0, assistantStartOffsetY),
                assistantOriginalPosition,
                smooth
            );

            float bounce = Mathf.Sin(p * Mathf.PI) * 0.08f;
            float scale = Mathf.Lerp(
                assistantStartScale,
                assistantEndScale,
                smooth
            ) + bounce;

            assistant.localScale = Vector3.one * scale;

            float yRot = Mathf.Lerp(totalRotation, 0f, smooth);
            assistant.localRotation = Quaternion.Euler(0f, yRot, 0f);

            if (sparklesRoot != null)
                sparklesRoot.anchoredPosition = assistant.anchoredPosition;

            AnimateAssistantSparkles(p);

            if (assistantGlow != null)
            {
                assistantGlow.anchoredPosition = assistant.anchoredPosition;
                assistantGlow.localScale =
                    Vector3.one * Mathf.Lerp(0.2f, 1.3f, smooth);

                if (glowCanvasGroup != null)
                    glowCanvasGroup.alpha =
                        Mathf.Sin(p * Mathf.PI) * 0.7f;
            }

            yield return null;
        }

        assistant.anchoredPosition = assistantOriginalPosition;
        assistant.localScale = Vector3.one;
        assistant.localRotation = Quaternion.identity;

        if (assistantCanvasGroup != null)
            assistantCanvasGroup.alpha = 1f;

        if (assistantGlow != null && glowCanvasGroup != null)
            glowCanvasGroup.alpha = 0f;

        SetSparklesAlpha(0f);

        if (sparklesRoot != null)
            sparklesRoot.gameObject.SetActive(false);

        if (idleRoutine != null)
            StopCoroutine(idleRoutine);

        idleRoutine = StartCoroutine(AssistantIdleLoop());
    }

    IEnumerator AssistantIdleLoop()
    {
        if (assistant == null)
            yield break;

        Vector2 basePos = assistantOriginalPosition;

        while (true)
        {
            float wave = Mathf.Sin(Time.time * idleFloatSpeed);
            float wave2 = Mathf.Sin(Time.time * idleFloatSpeed * 0.7f);

            assistant.anchoredPosition =
                basePos + new Vector2(0f, wave * idleFloatAmount);

            float zRot = wave2 * idleRotateAmount;
            assistant.localRotation = Quaternion.Euler(0f, 0f, zRot);

            float scale = 1f + Mathf.Abs(wave) * idleScaleAmount;
            assistant.localScale = Vector3.one * scale;

            yield return null;
        }
    }

    void AnimateAssistantSparkles(float progress)
    {
        if (assistantSparkles == null || assistantSparkles.Length == 0)
            return;

        float alpha = Mathf.Sin(progress * Mathf.PI);
        float timeAngle = Time.time * sparkleSpinSpeed;

        for (int i = 0; i < assistantSparkles.Length; i++)
        {
            RectTransform sparkle = assistantSparkles[i];
            if (sparkle == null) continue;

            CanvasGroup cg = sparkle.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = sparkle.gameObject.AddComponent<CanvasGroup>();

            float angle = timeAngle + (360f / assistantSparkles.Length) * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(rad) * sparkleRadius,
                Mathf.Sin(rad) * sparkleRadius
            );

            sparkle.anchoredPosition = pos;

            float pulse = 0.45f + Mathf.Sin(Time.time * 5f + i) * 0.12f;
            sparkle.localScale = Vector3.one * pulse;

            sparkle.Rotate(0f, 0f, 120f * Time.deltaTime);

            cg.alpha = alpha;
        }
    }

    void SetSparklesAlpha(float alpha)
    {
        if (assistantSparkles == null) return;

        foreach (RectTransform sparkle in assistantSparkles)
        {
            if (sparkle == null) continue;

            CanvasGroup cg = sparkle.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = sparkle.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = alpha;
        }
    }

    IEnumerator Pop(RectTransform target, float from, float to, float duration)
    {
        if (target == null) yield break;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float p = Mathf.Clamp01(t / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, p);
            float bounce = Mathf.Sin(p * Mathf.PI) * 0.12f;

            float scale = Mathf.Lerp(from, to, smooth) + bounce;
            target.localScale = Vector3.one * scale;

            yield return null;
        }

        target.localScale = Vector3.one * to;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;

            float p = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, p));

            yield return null;
        }

        canvasGroup.alpha = to;
    }

    IEnumerator ButtonPulse()
    {
        while (true)
        {
            yield return ScaleLoop(startButton, 1f, 1.05f, 0.7f);
            yield return ScaleLoop(startButton, 1.05f, 1f, 0.7f);
        }
    }

    IEnumerator StarsLoop()
    {
        while (true)
        {
            foreach (RectTransform star in magicStars)
            {
                if (star == null) continue;

                star.Rotate(0f, 0f, 18f * Time.deltaTime);

                float pulse =
                    1f + Mathf.Sin(Time.time * 2f) * 0.06f;

                star.localScale = Vector3.one * pulse;
            }

            yield return null;
        }
    }

    IEnumerator ScaleLoop(RectTransform target, float from, float to, float duration)
    {
        if (target == null) yield break;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float p = Mathf.Clamp01(t / duration);
            float scale = Mathf.Lerp(
                from,
                to,
                Mathf.SmoothStep(0f, 1f, p)
            );

            target.localScale = Vector3.one * scale;

            yield return null;
        }
    }
}