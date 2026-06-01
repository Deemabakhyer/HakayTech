using UnityEngine;
using UnityEngine.UI;

public class ScreenBubbleImageController : MonoBehaviour
{
    [Header("Target")]
    public Image bubbleImage;

    [Header("Bubble Sprites")]
    public Sprite firstBubbleSprite;
    public Sprite secondBubbleSprite;

    void Awake()
    {
        if (bubbleImage == null)
            bubbleImage = GetComponent<Image>();
    }

    public void ShowFirstBubbleImage()
    {
        if (bubbleImage != null && firstBubbleSprite != null)
            bubbleImage.sprite = firstBubbleSprite;
    }

    public void ShowSecondBubbleImage()
    {
        if (bubbleImage != null && secondBubbleSprite != null)
            bubbleImage.sprite = secondBubbleSprite;
    }
}