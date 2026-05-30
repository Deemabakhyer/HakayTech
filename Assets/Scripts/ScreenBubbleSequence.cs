using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenBubbleSequence : MonoBehaviour
{
    [Header("AI Visual Only")]
    public Image aiMentorImage;          // صورة الـ AI فقط
    public GameObject dialogueBubble;    // الفقاعة
    public ScreenBubbleImageController bubbleController;
    public Animator bubbleAnimator;

    [Header("Player Spawn")]
    public PlayerSpawnIntro playerSpawnIntro;

    [Header("Timing")]
    public float firstBubbleVisibleTime = 2.2f;
    public float delayBetweenBubbles = 0.35f;
    public float secondBubbleVisibleTime = 2.2f;
    public float delayBeforeHideAll = 0.8f;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (aiMentorImage != null)
            aiMentorImage.enabled = true;

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        if (bubbleController != null)
            bubbleController.ShowFirstBubbleImage();

        ShowBubbleWithAnimation();

        yield return new WaitForSeconds(firstBubbleVisibleTime);

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);

        yield return new WaitForSeconds(delayBetweenBubbles);

        if (bubbleController != null)
            bubbleController.ShowSecondBubbleImage();

        ShowBubbleWithAnimation();

        yield return new WaitForSeconds(secondBubbleVisibleTime);
        yield return new WaitForSeconds(delayBeforeHideAll);

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);

        if (aiMentorImage != null)
            aiMentorImage.enabled = false;

        if (playerSpawnIntro != null)
            playerSpawnIntro.PlaySpawn();
    }

    void ShowBubbleWithAnimation()
    {
        if (dialogueBubble != null)
            dialogueBubble.SetActive(true);

        if (bubbleAnimator != null)
        {
            bubbleAnimator.Rebind();
            bubbleAnimator.Update(0f);

            AnimatorStateInfo stateInfo = bubbleAnimator.GetCurrentAnimatorStateInfo(0);
            bubbleAnimator.Play(stateInfo.fullPathHash, 0, 0f);
        }
    }
}