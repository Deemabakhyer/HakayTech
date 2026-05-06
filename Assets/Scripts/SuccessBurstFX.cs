using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuccessBurstFX : MonoBehaviour
{
    [System.Serializable]
    public class BurstPiece
    {
        public RectTransform rect;
        public Vector2 burstOffset = new Vector2(80, 80);
        public float startRotation = 0;
        public float endRotation = 20;
        public float startScale = 0.2f;
        public float peakScale = 1.15f;
    }

    public CanvasGroup canvasGroup;
    public List<BurstPiece> pieces = new List<BurstPiece>();

    public float burstDuration = 0.35f;
    public float holdDuration = 0.35f;
    public float fadeDuration = 0.3f;

    private List<Vector2> basePositions = new List<Vector2>();
    private List<Vector3> baseScales = new List<Vector3>();

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Cache();
        Hide();
    }

    void Cache()
    {
        basePositions.Clear();
        baseScales.Clear();

        foreach (var p in pieces)
        {
            if (p.rect == null)
            {
                basePositions.Add(Vector2.zero);
                baseScales.Add(Vector3.one);
            }
            else
            {
                basePositions.Add(p.rect.anchoredPosition);
                baseScales.Add(p.rect.localScale);
            }
        }
    }

    public void PlayBurst()
    {
        Debug.Log("SUCCESS BURST PLAYED");

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        Cache();

        StopAllCoroutines();
        StartCoroutine(BurstRoutine());
    }

    IEnumerator BurstRoutine()
    {
        canvasGroup.alpha = 1f;

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].rect == null) continue;

            pieces[i].rect.gameObject.SetActive(true);

            Image img = pieces[i].rect.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }

            pieces[i].rect.anchoredPosition = basePositions[i];
            pieces[i].rect.localScale = baseScales[i] * pieces[i].startScale;
            pieces[i].rect.localRotation = Quaternion.Euler(0, 0, pieces[i].startRotation);
        }

        float t = 0f;

        while (t < burstDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, t / burstDuration);

            for (int i = 0; i < pieces.Count; i++)
            {
                var p = pieces[i];
                if (p.rect == null) continue;

                p.rect.anchoredPosition = Vector2.Lerp(basePositions[i], basePositions[i] + p.burstOffset, k);
                p.rect.localScale = Vector3.Lerp(baseScales[i] * p.startScale, baseScales[i] * p.peakScale, k);
                p.rect.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(p.startRotation, p.endRotation, k));
            }

            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        Hide();
    }

    void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}