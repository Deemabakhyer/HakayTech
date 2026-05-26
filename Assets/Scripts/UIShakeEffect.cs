using System.Collections;
using UnityEngine;

public class UIShakeEffect : MonoBehaviour
{
    public float duration = 0.25f;
    public float strength = 8f;

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float x = Random.Range(-strength, strength);
            float y = Random.Range(-strength, strength);

            transform.localPosition =
                originalPosition + new Vector3(x, y, 0f);

            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}