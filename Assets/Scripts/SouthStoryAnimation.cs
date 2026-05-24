using System.Collections;
using UnityEngine;
    public class SouthStoryAnimation : MonoBehaviour
{
    [Header("Main Panel")]
    public RectTransform storyPanel;
    public CanvasGroup storyPanelGroup;

    [Header("UI Elements to Animate")]
    public RectTransform closeButton;
    public RectTransform title;
    public RectTransform iconsRow;
    public RectTransform description;
    public RectTransform previewFrame;
    public RectTransform startButton;

    [Header("Optional Visual Effects")]
    public CanvasGroup dimOverlayGroup;
    public RectTransform panelGlow;
    public CanvasGroup panelGlowGroup;

    [Header("Audio (Intro Effect Only)")]
    public AudioSource magicWhoosh;
    public AudioClip showClip;

    [Header("Timing Settings")]
    public float startDelay = 0.15f;
    public float panelDuration = 0.65f;
    public float itemDuration = 0.28f;
    public float itemDelay = 0.07f;

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        Setup();
        Prepare();

        yield return new WaitForSeconds(startDelay);

        // 1. تشغيل صوت الظهور
        if (magicWhoosh != null && showClip != null)
            magicWhoosh.PlayOneShot(showClip);

        // 2. تعتيم الخلفية (Dim Overlay)
        yield return FadeDim();

        // 3. ظهور اللوحة الرئيسية (Story Panel)
        yield return ShowPanel();

        // 4. ظهور زر الإغلاق (Close Button)
        yield return ShowItem(closeButton, new Vector2(-25, 25));
        yield return new WaitForSeconds(itemDelay);

        // 5. ظهور العنوان (Title)
        yield return ShowItem(title, new Vector2(0, 35));
        yield return new WaitForSeconds(itemDelay);

        // 6. ظهور صف الأيقونات (Icons Row)
        yield return ShowItem(iconsRow, new Vector2(0, 25));
        yield return new WaitForSeconds(itemDelay);

        // 7. ظهور الوصف (Description)
        yield return ShowItem(description, new Vector2(0, 25));
        yield return new WaitForSeconds(itemDelay);

        // 8. ظهور إطار العرض (Preview Frame)
        yield return ShowItem(previewFrame, new Vector2(-35, 0));
        yield return new WaitForSeconds(itemDelay);

        // 9. ظهور زر ابدأ (Start Button)
        yield return ShowItem(startButton, new Vector2(0, -35));

        // 10. تفعيل التأثيرات المستمرة (النبض والتوهج)
        StartCoroutine(StartButtonPulse());
        StartCoroutine(PanelGlowLoop());
    }

    // --- الدوال المساعدة (تأكدي أنكِ لم تعدلي في داخلها) ---

    void Setup()
    {
        if (storyPanelGroup == null && storyPanel != null)
        {
            storyPanelGroup = storyPanel.GetComponent<CanvasGroup>();
            if (storyPanelGroup == null)
                storyPanelGroup = storyPanel.gameObject.AddComponent<CanvasGroup>();
        }

        if (panelGlow != null && panelGlowGroup == null)
        {
            panelGlowGroup = panelGlow.GetComponent<CanvasGroup>();
            if (panelGlowGroup == null)
                panelGlowGroup = panelGlow.gameObject.AddComponent<CanvasGroup>();
        }

        if (dimOverlayGroup == null)
        {
            GameObject dim = GameObject.Find("DimOverlay");
            if (dim != null)
                dimOverlayGroup = dim.GetComponent<CanvasGroup>();
        }
    }

    void Prepare()
    {
        if (storyPanel != null) storyPanel.localScale = Vector3.one * 0.78f;
        if (storyPanelGroup != null)
        {
            storyPanelGroup.alpha = 0f;
            storyPanelGroup.interactable = false;
            storyPanelGroup.blocksRaycasts = false;
        }
        if (dimOverlayGroup != null) dimOverlayGroup.alpha = 0f;

        // تجهيز كل العناصر لتكون مخفية (حجمها صفر)
        PrepareItem(closeButton);
        PrepareItem(title);
        PrepareItem(iconsRow);
        PrepareItem(description);
        PrepareItem(previewFrame);
        PrepareItem(startButton);

        if (panelGlowGroup != null) panelGlowGroup.alpha = 0f;
    }

    void PrepareItem(RectTransform item)
    {
        if (item == null) return;
        item.localScale = Vector3.zero;
    }

    IEnumerator FadeDim()
    {
        if (dimOverlayGroup == null) yield break;
        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            dimOverlayGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.35f);
            yield return null;
        }
        dimOverlayGroup.alpha = 1f;
    }

    IEnumerator ShowPanel()
    {
        if (storyPanel == null) yield break;
        Vector2 originalPos = storyPanel.anchoredPosition;
        float t = 0f;
        while (t < panelDuration)
        {
            t += Time.deltaTime;
            float p = t / panelDuration;
            float smooth = Mathf.SmoothStep(0, 1, p);
            storyPanel.localScale = Vector3.one * Mathf.Lerp(0.78f, 1f, smooth);
            storyPanel.anchoredPosition = Vector2.Lerp(originalPos + new Vector2(0, -45), originalPos, smooth);
            if (storyPanelGroup != null) storyPanelGroup.alpha = smooth;
            yield return null;
        }
        storyPanel.localScale = Vector3.one;
        storyPanel.anchoredPosition = originalPos;
        if (storyPanelGroup != null)
        {
            storyPanelGroup.alpha = 1f;
            storyPanelGroup.interactable = true;
            storyPanelGroup.blocksRaycasts = true;
        }
    }

    IEnumerator ShowItem(RectTransform item, Vector2 offset)
    {
        if (item == null) yield break;
        Vector2 originalPos = item.anchoredPosition;
        float t = 0f;
        while (t < itemDuration)
        {
            t += Time.deltaTime;
            float p = t / itemDuration;
            float smooth = Mathf.SmoothStep(0, 1, p);
            float bounce = Mathf.Sin(p * Mathf.PI) * 0.12f; // تأثير ارتداد خفيف
            item.localScale = Vector3.one * (smooth + bounce);
            item.anchoredPosition = Vector2.Lerp(originalPos + offset, originalPos, smooth);
            yield return null;
        }
        item.localScale = Vector3.one;
        item.anchoredPosition = originalPos;
    }

    IEnumerator StartButtonPulse()
    {
        if (startButton == null) yield break;
        while (true)
        {
            yield return Scale(startButton, Vector3.one, Vector3.one * 1.05f, 0.8f);
            yield return Scale(startButton, Vector3.one * 1.05f, Vector3.one, 0.8f);
        }
    }

    IEnumerator PanelGlowLoop()
    {
        if (panelGlowGroup == null) yield break;
        while (true)
        {
            yield return FadeGlow(0f, 0.1f, 1f);
            yield return FadeGlow(0.1f, 0f, 1f);
        }
    }

    IEnumerator FadeGlow(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            panelGlowGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        panelGlowGroup.alpha = to;
    }

    IEnumerator Scale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        target.localScale = to;
    }
}