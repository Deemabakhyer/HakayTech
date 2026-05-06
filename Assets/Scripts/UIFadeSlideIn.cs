using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFadeSlideIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    [Header("Animation")]
    public float duration = 0.7f;
    public Vector2 startOffset = new Vector2(0f, -80f);
    public float startScale = 0.85f;

    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Coroutine routine;

    void Awake()
    {
        Setup();
    }

    void OnEnable()
    {
        Setup();
        Play();
    }

    void Setup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
        }
    }

    public void Play()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPosition + startOffset;
        rectTransform.localScale = originalScale * startScale;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            float smooth = Mathf.SmoothStep(0f, 1f, p);

            canvasGroup.alpha = smooth;
            rectTransform.anchoredPosition =
                Vector2.Lerp(originalPosition + startOffset, originalPosition, smooth);

            rectTransform.localScale =
                Vector3.Lerp(originalScale * startScale, originalScale, smooth);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
    }
}