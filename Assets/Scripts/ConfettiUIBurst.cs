using System.Collections;
using UnityEngine;

public class ConfettiUIBurst : MonoBehaviour
{
    [Header("References")]
    public RectTransform leftConfetti;
    public RectTransform rightConfetti;

    public CanvasGroup leftCanvasGroup;
    public CanvasGroup rightCanvasGroup;

    [Header("Animation")]
    public float moveDistance = 120f;
    public float burstDuration = 0.4f;
    public float holdDuration = 0.5f;
    public float fadeDuration = 0.3f;

    private Vector2 leftEndPos;
    private Vector2 rightEndPos;

    void Awake()
    {
        if (leftConfetti != null)
            leftEndPos = leftConfetti.anchoredPosition;

        if (rightConfetti != null)
            rightEndPos = rightConfetti.anchoredPosition;

        ResetVisuals();
    }

    public void PlayBurst()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        ResetVisuals();

        Vector2 leftStart = leftEndPos + new Vector2(-moveDistance, 0f);
        Vector2 rightStart = rightEndPos + new Vector2(moveDistance, 0f);

        if (leftConfetti != null)
            leftConfetti.anchoredPosition = leftStart;

        if (rightConfetti != null)
            rightConfetti.anchoredPosition = rightStart;

        float t = 0f;
        while (t < burstDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / burstDuration);

            if (leftConfetti != null)
                leftConfetti.anchoredPosition = Vector2.Lerp(leftStart, leftEndPos, k);

            if (rightConfetti != null)
                rightConfetti.anchoredPosition = Vector2.Lerp(rightStart, rightEndPos, k);

            if (leftCanvasGroup != null)
                leftCanvasGroup.alpha = Mathf.Lerp(0f, 1f, k);

            if (rightCanvasGroup != null)
                rightCanvasGroup.alpha = Mathf.Lerp(0f, 1f, k);

            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        float f = 0f;
        while (f < fadeDuration)
        {
            f += Time.deltaTime;
            float k = f / fadeDuration;

            if (leftCanvasGroup != null)
                leftCanvasGroup.alpha = Mathf.Lerp(1f, 0f, k);

            if (rightCanvasGroup != null)
                rightCanvasGroup.alpha = Mathf.Lerp(1f, 0f, k);

            yield return null;
        }

        ResetVisuals();
    }

    void ResetVisuals()
    {
        if (leftCanvasGroup != null)
            leftCanvasGroup.alpha = 0f;

        if (rightCanvasGroup != null)
            rightCanvasGroup.alpha = 0f;

        if (leftConfetti != null)
            leftConfetti.anchoredPosition = leftEndPos;

        if (rightConfetti != null)
            rightConfetti.anchoredPosition = rightEndPos;
    }
}