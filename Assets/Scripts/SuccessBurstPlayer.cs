using System.Collections;
using UnityEngine;

public class SuccessBurstPlayer : MonoBehaviour
{
    [Header("Celebration Root")]
    public GameObject celebrationRoot;
    public AudioSource celebrationAudio;

    [Header("Burst Pieces")]
    public RectTransform[] pieces;

    [Header("Timing")]
    public float showDuration = 2.8f;
    public float burstDuration = 0.9f;
    public float fallDuration = 1.8f;

    [Header("Movement")]
    public float burstPower = 260f;
    public float fallDistance = 420f;
    public float rotationSpeed = 480f;

    private Vector2[] startPositions;
    private Vector3[] startScales;
    private bool played = false;

    private void Awake()
    {
        SaveStartState();

        if (celebrationRoot != null)
            celebrationRoot.SetActive(false);
    }

    private void SaveStartState()
    {
        if (pieces == null) return;

        startPositions = new Vector2[pieces.Length];
        startScales = new Vector3[pieces.Length];

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            startPositions[i] = pieces[i].anchoredPosition;
            startScales[i] = pieces[i].localScale;
        }
    }

    public void PlayCelebration()
    {
        if (played) return;

        played = true;
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (celebrationRoot != null)
            celebrationRoot.SetActive(true);

        if (celebrationAudio != null)
            celebrationAudio.Play();

        ResetPieces();

        yield return StartCoroutine(BurstOut());
        yield return StartCoroutine(FallDown());

        yield return new WaitForSeconds(0.2f);

        if (celebrationRoot != null)
            celebrationRoot.SetActive(false);
    }

    private void ResetPieces()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            pieces[i].gameObject.SetActive(true);
            pieces[i].anchoredPosition = startPositions[i];
            pieces[i].localScale = startScales[i];
            pieces[i].localRotation = Quaternion.identity;
        }
    }

    private IEnumerator BurstOut()
    {
        float timer = 0f;

        Vector2[] targetPositions = new Vector2[pieces.Length];

        for (int i = 0; i < pieces.Length; i++)
        {
            float angle = Mathf.Lerp(20f, 160f, (float)i / Mathf.Max(1, pieces.Length - 1));
            float rad = angle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            targetPositions[i] = startPositions[i] + direction * burstPower;
        }

        while (timer < burstDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / burstDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] == null) continue;

                pieces[i].anchoredPosition =
                    Vector2.Lerp(startPositions[i], targetPositions[i], smooth);

                pieces[i].localRotation =
                    Quaternion.Euler(0f, 0f, rotationSpeed * t * ((i % 2 == 0) ? 1 : -1));
            }

            yield return null;
        }
    }

    private IEnumerator FallDown()
    {
        float timer = 0f;

        Vector2[] fallStart = new Vector2[pieces.Length];

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            fallStart[i] = pieces[i].anchoredPosition;
        }

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fallDuration);

            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] == null) continue;

                float sway = Mathf.Sin((t * 8f) + i) * 35f;

                pieces[i].anchoredPosition =
                    fallStart[i] + new Vector2(sway, -fallDistance * t);

                pieces[i].localRotation =
                    Quaternion.Euler(0f, 0f, pieces[i].localRotation.eulerAngles.z + rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }
    }
}