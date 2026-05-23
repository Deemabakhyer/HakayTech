using System.Collections;
using UnityEngine;

public class BlockPopEffect : MonoBehaviour
{
    public float smallScale = 0.92f;
    public float bigScale = 1.06f;
    public float duration = 0.12f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlayPop()
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        transform.localScale = originalScale * smallScale;

        yield return ScaleTo(originalScale * bigScale, duration);
        yield return ScaleTo(originalScale, duration);
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float time)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / time);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
