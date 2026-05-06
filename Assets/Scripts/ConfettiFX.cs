using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConfettiFX : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.4f;
    public float showDuration = 1.2f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    public void Play()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        yield return Fade(0, 1);

        yield return new WaitForSeconds(showDuration);

        yield return Fade(1, 0);

        gameObject.SetActive(false);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            canvasGroup.alpha = a;
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
