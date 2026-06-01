using System.Collections;
using UnityEngine;

public class GoldenFlashEffect : MonoBehaviour
{
    public CanvasGroup flashCanvasGroup;

    public float maxAlpha = 0.35f;
    public float fadeInTime = 0.12f;
    public float fadeOutTime = 0.35f;

    private void Awake()
    {
        if (flashCanvasGroup == null)
            flashCanvasGroup = GetComponent<CanvasGroup>();

        flashCanvasGroup.alpha = 0f;
    }

    public void PlayFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;

        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            flashCanvasGroup.alpha = Mathf.Lerp(0f, maxAlpha, timer / fadeInTime);
            yield return null;
        }

        timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            flashCanvasGroup.alpha = Mathf.Lerp(maxAlpha, 0f, timer / fadeOutTime);
            yield return null;
        }

        flashCanvasGroup.alpha = 0f;
    }
}