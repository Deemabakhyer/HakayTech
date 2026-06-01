using System.Collections;
using UnityEngine;

public class BlockExecutionController : MonoBehaviour
{
    [Header("Character Inside Screen")]
    public Transform character;

    [Header("Waypoints")]
    public Transform pointStart;
    public Transform pointForward1;
    public Transform pointStop;
    public Transform pointForward2;
    public Transform pointEnd;

    [Header("Controllers")]
    public AIDialogueController aiDialogue;
    public RScreenStateVisual screenVisual;

    [Header("Boy States (9 States)")]
    public GameObject boyPressStart;
    public GameObject boySurprised;
    public GameObject boyWelcomeHologram;
    public GameObject boyReady;
    public GameObject boyPressHologram;
    public GameObject boyPressContinue;
    public GameObject boyThinking;
    public GameObject boyPressFix;
    public GameObject boyCelebrate;

    [Header("Movement Settings")]
    public float moveDuration = 0.6f;
    public float rotateDuration = 0.25f;
    public float stopDuration = 0.8f;

    [Header("Story Timing")]
    public float pressStartDelay = 0.4f;
    public float surprisedDelay = 0.7f;
    public float welcomeDelay = 0.7f;
    public float readyDelay = 0.6f;
    public float pressHologramDelay = 0.5f;
    public float pressContinueDelay = 0.5f;
    public float beforeWrongTurnDelay = 0.25f;
    public float afterWrongMoveDelay = 0.8f;
    public float beforeFixDelay = 0.25f;
    public float afterFixDelay = 0.2f;
    public float pressFixDelay = 0.6f;
    public float thinkingWaitAfterEffect = 0.6f;

    private bool isExecuting = false;
    private Coroutine currentRoutine;
    private float defaultMoveDuration;

    void Start()
    {
        defaultMoveDuration = moveDuration;
        ResetCharacter();
    }

    public void ResetCharacter()
    {
        StopCurrentSequence();

        if (character != null && pointStart != null)
        {
            character.position = pointStart.position;
            character.rotation = Quaternion.identity;
        }

        moveDuration = defaultMoveDuration;

        if (screenVisual != null)
            screenVisual.ShowNormal();

        if (aiDialogue != null)
            aiDialogue.ShowState(1);

        ShowOnlyBoyState(boyPressStart);
    }

    public void StopCurrentSequence()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        StopAllCoroutines();
        isExecuting = false;
        moveDuration = defaultMoveDuration;
    }

    public void PlayStorySequence()
    {
        if (isExecuting) return;
        currentRoutine = StartCoroutine(StoryRoutine());
    }

    IEnumerator StoryRoutine()
    {
        isExecuting = true;

        ResetTransformOnly();

        if (screenVisual != null)
            screenVisual.ShowNormal();

        if (aiDialogue != null)
            aiDialogue.ShowState(12);

        ShowOnlyBoyState(boyPressStart);
        yield return new WaitForSeconds(pressStartDelay);

        ShowOnlyBoyState(boySurprised);
        yield return new WaitForSeconds(surprisedDelay);

        ShowOnlyBoyState(boyWelcomeHologram);
        yield return new WaitForSeconds(welcomeDelay);

        ShowOnlyBoyState(boyReady);
        yield return new WaitForSeconds(readyDelay);

        ShowOnlyBoyState(boyPressHologram);
        yield return new WaitForSeconds(pressHologramDelay);

        ShowOnlyBoyState(boyPressContinue);
        yield return new WaitForSeconds(pressContinueDelay);

        yield return MoveTo(pointForward1);

        yield return new WaitForSeconds(beforeWrongTurnDelay);

        yield return RotateTo(-90f);

        if (screenVisual != null)
            screenVisual.ShowError();

        if (aiDialogue != null)
            aiDialogue.ShowState(13);

        if (pointStop != null)
            yield return MoveTo(pointStop);

        yield return new WaitForSeconds(afterWrongMoveDelay);

        ShowOnlyBoyState(boyThinking);

        if (aiDialogue != null)
            aiDialogue.ShowState(8);

        yield return StartCoroutine(ThinkingEffect());
        yield return new WaitForSeconds(thinkingWaitAfterEffect);

        if (screenVisual != null)
            screenVisual.ShowRecover();

        ShowOnlyBoyState(boyPressFix);

        if (aiDialogue != null)
            aiDialogue.ShowState(9);

        yield return new WaitForSeconds(pressFixDelay);
        yield return new WaitForSeconds(beforeFixDelay);

        yield return RotateTo(0f);
        yield return new WaitForSeconds(afterFixDelay);

        if (screenVisual != null)
            screenVisual.ShowSuccessGlow();

        if (aiDialogue != null)
            aiDialogue.ShowState(14);

        yield return MoveTo(pointForward2);

        moveDuration = 0.45f;

        if (pointEnd != null)
            yield return MoveTo(pointEnd);

        moveDuration = defaultMoveDuration;

        if (screenVisual != null)
            screenVisual.PlaySuccessSequence();

        if (aiDialogue != null)
            aiDialogue.ShowState(15);

        ShowOnlyBoyState(boyCelebrate);

        isExecuting = false;
        currentRoutine = null;
    }

    IEnumerator MoveTo(Transform target)
    {
        if (character == null || target == null)
            yield break;

        Vector3 start = character.position;
        Vector3 end = target.position;

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / moveDuration);
            character.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        character.position = end;
    }

    IEnumerator RotateTo(float targetZ)
    {
        if (character == null)
            yield break;

        float startZ = character.eulerAngles.z;
        if (startZ > 180f) startZ -= 360f;

        float t = 0f;
        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / rotateDuration);
            float z = Mathf.Lerp(startZ, targetZ, k);
            character.rotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        character.rotation = Quaternion.Euler(0f, 0f, targetZ);
    }

    IEnumerator ThinkingEffect()
    {
        if (character == null)
            yield break;

        Vector3 basePos = character.position;
        float duration = 0.4f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float offsetX = Mathf.Sin(t * 20f) * 0.05f;
            character.position = basePos + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        character.position = basePos;
    }

    void ResetTransformOnly()
    {
        if (character != null && pointStart != null)
        {
            character.position = pointStart.position;
            character.rotation = Quaternion.identity;
        }
    }

    void ShowOnlyBoyState(GameObject active)
    {
        if (boyPressStart != null) boyPressStart.SetActive(false);
        if (boySurprised != null) boySurprised.SetActive(false);
        if (boyWelcomeHologram != null) boyWelcomeHologram.SetActive(false);
        if (boyReady != null) boyReady.SetActive(false);
        if (boyPressHologram != null) boyPressHologram.SetActive(false);
        if (boyPressContinue != null) boyPressContinue.SetActive(false);
        if (boyThinking != null) boyThinking.SetActive(false);
        if (boyPressFix != null) boyPressFix.SetActive(false);
        if (boyCelebrate != null) boyCelebrate.SetActive(false);

        if (active != null)
            active.SetActive(true);
    }
}