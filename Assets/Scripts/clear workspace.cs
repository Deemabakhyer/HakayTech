using UnityEngine;

public class TrashDropArea : MonoBehaviour
{
    public static TrashDropArea Instance;

    public AudioSource audioSource;
    public AudioClip trashClip;

    private RectTransform rectTransform;

    private void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
    }

    public bool IsPointerInside(Vector2 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            screenPosition,
            null
        );
    }

    public void DeleteBlock(GameObject block)
    {
        BlockDragHandler drag = block.GetComponent<BlockDragHandler>();

        if (drag != null && drag.isTemplate)
            return;

        if (audioSource != null && trashClip != null)
            audioSource.PlayOneShot(trashClip);

        Destroy(block);
    }
}