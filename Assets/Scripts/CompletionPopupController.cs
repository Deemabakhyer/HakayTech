using System.Collections;
using UnityEngine;

public class CompletionPopupController : MonoBehaviour
{
    [Header("Popup")]
    public GameObject popupRoot;
    public RectTransform panel;

    [Header("Stars")]
    public RectTransform star1;
    public RectTransform star2;
    public RectTransform star3;

    [Header("Timing")]
    public float showDelay = 0.8f;
    public float popupDuration = 0.35f;
    public float starDelay = 0.18f;
    public float starPopDuration = 0.25f;

    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip starClip;

    void Awake()
    {
        if (popupRoot == null)
            popupRoot = gameObject;

        ResetVisualsOnly();
    }

    public void ShowPopup()
    {
        if (popupRoot == null)
            popupRoot = gameObject;

        popupRoot.SetActive(true);
        popupRoot.transform.SetAsLastSibling();

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    public void HideInstant()
    {
        if (popupRoot == null)
            popupRoot = gameObject;

        ResetVisualsOnly();
        popupRoot.SetActive(false);
    }

    private void ResetVisualsOnly()
    {
        if (panel != null)
            panel.localScale = Vector3.zero;

        SetStarScale(star1, 0f);
        SetStarScale(star2, 0f);
        SetStarScale(star3, 0f);
    }

    IEnumerator ShowRoutine()
    {
        ResetVisualsOnly();

        yield return new WaitForSeconds(showDelay);

        yield return Pop(panel, 0f, 1f, popupDuration);

        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star1);

        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star2);

        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star3);
    }

    IEnumerator PopStar(RectTransform star)
    {
        if (audioSource != null && starClip != null)
            audioSource.PlayOneShot(starClip);

        yield return Pop(star, 0f, 1f, starPopDuration);
    }

    IEnumerator Pop(RectTransform target, float from, float to, float duration)
    {
        if (target == null)
            yield break;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            float smooth = Mathf.SmoothStep(0f, 1f, p);
            float overshoot = Mathf.Sin(p * Mathf.PI) * 0.12f;
            float scale = Mathf.Lerp(from, to, smooth) + overshoot;

            target.localScale = Vector3.one * scale;

            yield return null;
        }

        target.localScale = Vector3.one * to;
    }

    private void SetStarScale(RectTransform star, float scale)
    {
        if (star != null)
            star.localScale = Vector3.one * scale;
    }
}
