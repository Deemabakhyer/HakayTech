using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RScreenStateVisual : MonoBehaviour
{
    public enum VisualState
    {
        Normal,
        Error,
        Recover,
        SuccessGlow,
        Completed
    }

    public Image targetImage;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite errorSprite;
    public Sprite recoverSprite;
    public Sprite successGlowSprite;
    public Sprite completedPathSprite;

    public VisualState CurrentState { get; private set; } = VisualState.Normal;

    public void ShowNormal()
    {
        CurrentState = VisualState.Normal;
        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;
    }

    public void ShowError()
    {
        CurrentState = VisualState.Error;
        if (targetImage != null && errorSprite != null)
            targetImage.sprite = errorSprite;
    }

    public void ShowRecover()
    {
        CurrentState = VisualState.Recover;
        if (targetImage != null && recoverSprite != null)
            targetImage.sprite = recoverSprite;
    }

    public void ShowSuccessGlow()
    {
        CurrentState = VisualState.SuccessGlow;
        if (targetImage != null && successGlowSprite != null)
            targetImage.sprite = successGlowSprite;
    }

    public void ShowCompletedPath()
    {
        CurrentState = VisualState.Completed;
        if (targetImage != null && completedPathSprite != null)
            targetImage.sprite = completedPathSprite;
    }

    public void PlaySuccessSequence(float delayBeforeCompleted = 0.55f)
    {
        StopAllCoroutines();
        StartCoroutine(SuccessSequenceRoutine(delayBeforeCompleted));
    }

    IEnumerator SuccessSequenceRoutine(float delayBeforeCompleted)
    {
        ShowSuccessGlow();
        yield return new WaitForSeconds(delayBeforeCompleted);
        ShowCompletedPath();
    }
}