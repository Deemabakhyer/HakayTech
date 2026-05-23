using System.Collections;
using UnityEngine;

public class PopupCardAnimator : MonoBehaviour
{
    [Header("Popup Card")]
    public RectTransform popupCard;

    [Header("Animation")]
    public float startScale = 0.85f;
    public float overshootScale = 1.05f;
    public float animationTime = 0.15f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (popupCard != null)
            originalScale = popupCard.localScale;
    }

    private void OnEnable()
    {
        if (popupCard != null)
        {
            StopAllCoroutines();
            StartCoroutine(OpenAnimation());
        }
    }

    private IEnumerator OpenAnimation()
    {
        popupCard.localScale = originalScale * startScale;

        float timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / animationTime;

            float scale = Mathf.Lerp(startScale, overshootScale, t);

            popupCard.localScale = originalScale * scale;

            yield return null;
        }

        timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / animationTime;

            float scale = Mathf.Lerp(overshootScale, 1f, t);

            popupCard.localScale = originalScale * scale;

            yield return null;
        }

        popupCard.localScale = originalScale;
    }
}