using System.Collections;
using UnityEngine;

/// <summary>
/// A generic animation controller for story introduction panels.
/// Handles smooth transitions, item sequencing, and visual effects.
/// </summary>
public class StoryIntroAnimation : MonoBehaviour
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

        yield return FadeDim();
        yield return ShowPanel();

        // Sequential appearance of elements
        yield return ShowItem(closeButton, new Vector2(-25, 25));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(title, new Vector2(0, 35));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(iconsRow, new Vector2(0, 25));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(description, new Vector2(0, 25));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(previewFrame, new Vector2(-35, 0));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(startButton, new Vector2(0, -35));

        // Start looping effects
        StartCoroutine(StartButtonPulse());
        StartCoroutine(PanelGlowLoop());
    }

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
        if (storyPanel != null)
            storyPanel.localScale = Vector3.one * 0.78f;

        if (storyPanelGroup != null)
        {
            storyPanelGroup.alpha = 0f;
            storyPanelGroup.interactable = false;
            storyPanelGroup.blocksRaycasts = false;
        }

        if (dimOverlayGroup != null)
            dimOverlayGroup.alpha = 0f;

        PrepareItem(closeButton);
        PrepareItem(title);
        PrepareItem(iconsRow);
        PrepareItem(description);
        PrepareItem(previewFrame);
        PrepareItem(startButton);

        if (panelGlowGroup != null)
            panelGlowGroup.alpha = 0f;
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
        float duration = 0.35f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            dimOverlayGroup.alpha = Mathf.Lerp(0f, 1f, Mathf.SmoothStep(0, 1, p));
            yield return null;
        }
        dimOverlayGroup.alpha = 1f;
    }

    IEnumerator ShowPanel()
    {
        if (storyPanel == null) yield break;
        Vector2 originalPos = storyPanel.anchoredPosition;
        storyPanel.anchoredPosition = originalPos + new Vector2(0, -45);
        float t = 0f;
        while (t < panelDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / panelDuration);
            float smooth = Mathf.SmoothStep(0, 1, p);
            float bounce = Mathf.Sin(p * Mathf.PI) * 0.08f;
            storyPanel.localScale = Vector3.one * (Mathf.Lerp(0.78f, 1f, smooth) + bounce);
            storyPanel.anchoredPosition = Vector2.Lerp(originalPos + new Vector2(0, -45), originalPos, smooth);
            if (storyPanelGroup != null) storyPanelGroup.alpha = smooth;
            if (panelGlowGroup != null) panelGlowGroup.alpha = Mathf.Sin(p * Mathf.PI) * 0.28f;
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
        item.anchoredPosition = originalPos + offset;
        item.localScale = Vector3.zero;
        float t = 0f;
        while (t < itemDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / itemDuration);
            float smooth = Mathf.SmoothStep(0, 1, p);
            float bounce = Mathf.Sin(p * Mathf.PI) * 0.12f;
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
            yield return Scale(startButton, Vector3.one, Vector3.one * 1.045f, 0.75f);
            yield return Scale(startButton, Vector3.one * 1.045f, Vector3.one, 0.75f);
        }
    }

    IEnumerator PanelGlowLoop()
    {
        if (panelGlowGroup == null) yield break;
        while (true)
        {
            yield return FadeGlow(0f, 0.08f, 0.9f);
            yield return FadeGlow(0.08f, 0f, 0.9f);
            yield return new WaitForSeconds(1.3f);
        }
    }

    IEnumerator FadeGlow(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            panelGlowGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, p));
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
            float p = Mathf.Clamp01(t / duration);
            target.localScale = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, p));
            yield return null;
        }
        target.localScale = to;
    }
}