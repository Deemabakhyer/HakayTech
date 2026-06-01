using System.Collections;
using UnityEngine;

public class MakeAreekaBoardSequence : MonoBehaviour
{
    [Header("References")]
    public StoryScenePlayer storyScenePlayer;
    public RectTransform boardRect;

    [Header("Plates")]
    public GameObject rightPlate;
    public RectTransform rightPlateRect;

    [Header("Final Positions")]
    public RectTransform rightFinalPosition;

    [Header("Audio")]
    public AudioSource boardClickAudio;
    public AudioSource magicHumAudio;
    public AudioSource areekaAppearAudio;
    public AudioSource sparkleAudio;

    [Header("Effects")]
    public GoldenFlashEffect goldenFlashEffect;

    [Header("Timing")]
    public float pressDuration = 0.12f;
    public float magicDelay = 0.15f;
    public float revealDuration = 0.9f;
    public float delayBeforeContinue = 0.45f;

    [Header("Motion")]
    public float startScale = 0.15f;
    public float middleScale = 1.15f;
    public float endScale = 1f;
    public float arcHeight = 120f;
    public float rotationAmount = 360f;

    private Vector3 boardOriginalScale;
    private CanvasGroup rightGroup;
    private bool alreadyClicked = false;

    private void Awake()
    {
        if (boardRect == null)
            boardRect = GetComponent<RectTransform>();

        if (boardRect != null)
            boardOriginalScale = boardRect.localScale;

        PreparePlate(rightPlate, out rightGroup);

        alreadyClicked = false;
    }

    private void OnEnable()
    {
        alreadyClicked = false;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (alreadyClicked) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(boardRect, Input.mousePosition, null))
            {
                PlayBoardSequence();
            }
        }
    }

    private void PreparePlate(GameObject plate, out CanvasGroup group)
    {
        group = null;

        if (plate == null) return;

        group = plate.GetComponent<CanvasGroup>();
        if (group == null)
            group = plate.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        plate.SetActive(false);
    }

    public void PlayBoardSequence()
    {
        if (alreadyClicked) return;

        alreadyClicked = true;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        StartCoroutine(BoardSequenceRoutine());
    }

    private IEnumerator BoardSequenceRoutine()
    {
        if (boardClickAudio != null)
            boardClickAudio.Play();

        yield return PressBoard();

        if (magicHumAudio != null)
            magicHumAudio.Play();

        yield return new WaitForSeconds(magicDelay);

        if (areekaAppearAudio != null)
            areekaAppearAudio.Play();

        yield return RevealPlate();

        if (sparkleAudio != null)
            sparkleAudio.Play();

        if (goldenFlashEffect != null)
            goldenFlashEffect.PlayFlash();

        yield return new WaitForSeconds(delayBeforeContinue);

        if (storyScenePlayer != null)
            storyScenePlayer.ContinueAfterBoardClick();
    }

    private IEnumerator PressBoard()
    {
        if (boardRect == null) yield break;

        boardRect.localScale = boardOriginalScale * 0.92f;
        yield return new WaitForSeconds(pressDuration);

        boardRect.localScale = boardOriginalScale * 1.06f;
        yield return new WaitForSeconds(pressDuration);

        boardRect.localScale = boardOriginalScale;
    }

    private IEnumerator RevealPlate()
    {
        if (rightPlate == null) yield break;
        if (rightPlateRect == null) yield break;
        if (rightFinalPosition == null) yield break;

        rightPlate.SetActive(true);

        Vector3 boardWorld = boardRect.position;

        Vector3 rightStart = boardWorld + new Vector3(60f, 0f, 0f);
        Vector3 rightEnd = rightFinalPosition.position;

        rightPlateRect.position = rightStart;
        rightPlateRect.localScale = Vector3.one * startScale;
        rightPlateRect.localRotation = Quaternion.identity;

        if (rightGroup != null) rightGroup.alpha = 0f;

        float timer = 0f;

        while (timer < revealDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / revealDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rightPlateRect.position = GetArcWorldPosition(rightStart, rightEnd, smooth, arcHeight);

            float popScale = Mathf.Lerp(startScale, middleScale, Mathf.Sin(smooth * Mathf.PI));
            float finalScale = Mathf.Lerp(popScale, endScale, smooth);

            rightPlateRect.localScale = Vector3.one * finalScale;
            rightPlateRect.localRotation = Quaternion.Euler(0f, 0f, rotationAmount * smooth);

            if (rightGroup != null) rightGroup.alpha = smooth;

            yield return null;
        }

        rightPlateRect.position = rightEnd;
        rightPlateRect.localScale = Vector3.one * endScale;
        rightPlateRect.localRotation = Quaternion.identity;

        if (rightGroup != null) rightGroup.alpha = 1f;

        StartCoroutine(PlateBounce(rightPlateRect));
    }

    private IEnumerator PlateBounce(RectTransform plate)
    {
        Vector3 originalScale = plate.localScale;

        plate.localScale = originalScale * 1.10f;
        yield return new WaitForSeconds(0.08f);

        plate.localScale = originalScale * 0.96f;
        yield return new WaitForSeconds(0.08f);

        plate.localScale = originalScale;
    }

    private Vector3 GetArcWorldPosition(Vector3 start, Vector3 end, float t, float height)
    {
        Vector3 position = Vector3.Lerp(start, end, t);
        position.y += Mathf.Sin(t * Mathf.PI) * height;
        return position;
    }
}