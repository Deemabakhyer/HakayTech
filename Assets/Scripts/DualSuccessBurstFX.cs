using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualSuccessBurstFX : MonoBehaviour
{
    [System.Serializable]
    public class BurstPiece
    {
        public RectTransform rect;
        public Vector2 burstOffset;
        public float startRotation;
        public float endRotation;
        public float startScale = 0.2f;
        public float peakScale = 1f;
    }

    [System.Serializable]
    public class BurstSide
    {
        public RectTransform root;
        public CanvasGroup canvasGroup;
        public List<BurstPiece> pieces = new List<BurstPiece>();
    }

    [Header("Sides")]
    public BurstSide leftSide;
    public BurstSide rightSide;

    [Header("Timing")]
    public float burstDuration = 0.32f;
    public float holdDuration = 0.45f;
    public float fallDuration = 0.45f;
    public float fadeDuration = 0.35f;

    [Header("Fall Motion")]
    public float fallDistance = 35f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successClip;

    private readonly List<Vector2> leftBasePositions = new List<Vector2>();
    private readonly List<Vector3> leftBaseScales = new List<Vector3>();

    private readonly List<Vector2> rightBasePositions = new List<Vector2>();
    private readonly List<Vector3> rightBaseScales = new List<Vector3>();

    void Awake()
    {
        CacheSide(leftSide, leftBasePositions, leftBaseScales);
        CacheSide(rightSide, rightBasePositions, rightBaseScales);

        ResetSide(leftSide, leftBasePositions, leftBaseScales);
        ResetSide(rightSide, rightBasePositions, rightBaseScales);
    }

    void CacheSide(BurstSide side, List<Vector2> posList, List<Vector3> scaleList)
    {
        posList.Clear();
        scaleList.Clear();

        foreach (var piece in side.pieces)
        {
            if (piece.rect == null)
            {
                posList.Add(Vector2.zero);
                scaleList.Add(Vector3.one);
                continue;
            }

            posList.Add(piece.rect.anchoredPosition);
            scaleList.Add(piece.rect.localScale);
        }
    }

    public void PlayBurst()
    {
        StopAllCoroutines();

        if (audioSource != null && successClip != null)
            audioSource.PlayOneShot(successClip);

        StartCoroutine(BurstRoutine());
    }

    IEnumerator BurstRoutine()
    {
        ResetSide(leftSide, leftBasePositions, leftBaseScales);
        ResetSide(rightSide, rightBasePositions, rightBaseScales);

        float t = 0f;
        while (t < burstDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / burstDuration);

            AnimateSideBurst(leftSide, leftBasePositions, leftBaseScales, k);
            AnimateSideBurst(rightSide, rightBasePositions, rightBaseScales, k);

            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fallDuration)
        {
            t += Time.deltaTime;
            float k = t / fallDuration;

            AnimateSideFall(leftSide, leftBasePositions, leftBaseScales, k);
            AnimateSideFall(rightSide, rightBasePositions, rightBaseScales, k);

            yield return null;
        }

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeDuration;

            if (leftSide.canvasGroup != null)
                leftSide.canvasGroup.alpha = Mathf.Lerp(1f, 0f, k);

            if (rightSide.canvasGroup != null)
                rightSide.canvasGroup.alpha = Mathf.Lerp(1f, 0f, k);

            yield return null;
        }

        ResetSide(leftSide, leftBasePositions, leftBaseScales);
        ResetSide(rightSide, rightBasePositions, rightBaseScales);
    }

    void AnimateSideBurst(BurstSide side, List<Vector2> basePos, List<Vector3> baseScale, float k)
    {
        for (int i = 0; i < side.pieces.Count; i++)
        {
            var piece = side.pieces[i];
            if (piece.rect == null) continue;

            Vector2 startPos = basePos[i];
            Vector2 endPos = basePos[i] + piece.burstOffset;
            piece.rect.anchoredPosition = Vector2.Lerp(startPos, endPos, k);

            float rot = Mathf.Lerp(piece.startRotation, piece.endRotation, k);
            piece.rect.localRotation = Quaternion.Euler(0f, 0f, rot);

            float scale = Mathf.Lerp(piece.startScale, piece.peakScale, k);
            piece.rect.localScale = baseScale[i] * scale;
        }

        if (side.canvasGroup != null)
            side.canvasGroup.alpha = Mathf.Lerp(0f, 1f, k);
    }

    void AnimateSideFall(BurstSide side, List<Vector2> basePos, List<Vector3> baseScale, float k)
    {
        for (int i = 0; i < side.pieces.Count; i++)
        {
            var piece = side.pieces[i];
            if (piece.rect == null) continue;

            Vector2 burstPos = basePos[i] + piece.burstOffset;
            Vector2 fallPos = burstPos + new Vector2(0f, -fallDistance);

            piece.rect.anchoredPosition = Vector2.Lerp(burstPos, fallPos, k);

            float rot = Mathf.Lerp(piece.endRotation, piece.endRotation + 12f, k);
            piece.rect.localRotation = Quaternion.Euler(0f, 0f, rot);

            float scale = Mathf.Lerp(piece.peakScale, piece.peakScale * 0.92f, k);
            piece.rect.localScale = baseScale[i] * scale;
        }

        if (side.canvasGroup != null)
            side.canvasGroup.alpha = 1f;
    }

    void ResetSide(BurstSide side, List<Vector2> basePos, List<Vector3> baseScale)
    {
        if (side.canvasGroup != null)
            side.canvasGroup.alpha = 0f;

        for (int i = 0; i < side.pieces.Count; i++)
        {
            var piece = side.pieces[i];
            if (piece.rect == null) continue;

            piece.rect.anchoredPosition = basePos[i];
            piece.rect.localRotation = Quaternion.Euler(0f, 0f, piece.startRotation);
            piece.rect.localScale = baseScale[i] * piece.startScale;
        }
    }
}