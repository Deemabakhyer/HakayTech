using System.Collections;
using UnityEngine;

public class SceneObjectPopIn : MonoBehaviour
{
    [Header("AI")]
    public RectTransform aiObject;

    [Header("Bubble Objects")]
    public RectTransform emptyBubble;
    public RectTransform textBubble;

    [Header("Timing")]
    public float aiDelay = 0.2f;
    public float bubbleDelay = 0.35f;
    public float animationTime = 0.22f;

    [Header("Scale")]
    public float startScale = 0.4f;
    public float overshootScale = 1.12f;

    private CanvasGroup aiGroup;
    private CanvasGroup emptyBubbleGroup;
    private CanvasGroup textBubbleGroup;

    private void OnEnable()
    {
        SetupObject(aiObject, ref aiGroup);
        SetupObject(emptyBubble, ref emptyBubbleGroup);
        SetupObject(textBubble, ref textBubbleGroup);

        StartCoroutine(PlaySequence());
    }

    private void SetupObject(RectTransform obj, ref CanvasGroup group)
    {
        if (obj == null)
            return;

        group = obj.GetComponent<CanvasGroup>();
        if (group == null)
            group = obj.gameObject.AddComponent<CanvasGroup>();

        obj.localScale = Vector3.one * startScale;
        group.alpha = 0f;
        obj.gameObject.SetActive(false);
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(aiDelay);

        if (aiObject != null)
            yield return PopIn(aiObject, aiGroup);

        yield return new WaitForSeconds(bubbleDelay);

        StartCoroutine(PopIn(emptyBubble, emptyBubbleGroup));
        StartCoroutine(PopIn(textBubble, textBubbleGroup));
    }

    private IEnumerator PopIn(RectTransform obj, CanvasGroup group)
    {
        if (obj == null)
            yield break;

        obj.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / animationTime);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            obj.localScale = Vector3.one * Mathf.Lerp(startScale, overshootScale, smooth);

            if (group != null)
                group.alpha = smooth;

            yield return null;
        }

        timer = 0f;

        while (timer < animationTime * 0.65f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / (animationTime * 0.65f));
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            obj.localScale = Vector3.one * Mathf.Lerp(overshootScale, 1f, smooth);

            yield return null;
        }

        obj.localScale = Vector3.one;

        if (group != null)
            group.alpha = 1f;
    }
}