using System.Collections;
using UnityEngine;

public class ButtonShine : MonoBehaviour
{
    public RectTransform shine;

    public float startX = -350f;
    public float endX = 350f;
    public float duration = 0.8f;
    public float waitTime = 1.8f;

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(ShineLoop());
    }

    IEnumerator ShineLoop()
    {
        while (true)
        {
            if (shine != null)
            {
                Vector2 pos = shine.anchoredPosition;
                pos.x = startX;
                shine.anchoredPosition = pos;

                float t = 0f;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    float p = Mathf.Clamp01(t / duration);
                    float smooth = Mathf.SmoothStep(0f, 1f, p);

                    pos.x = Mathf.Lerp(startX, endX, smooth);
                    shine.anchoredPosition = pos;

                    yield return null;
                }
            }

            yield return new WaitForSeconds(waitTime);
        }
    }
}