using UnityEngine;

/// <summary>
/// TrashDropArea: Manages the workspace trash disposal zone, validating 
/// pointer positioning, triggering deletion sound effects, and supporting multiple 
/// drag handler types across different environments using conditional compilation.
/// </summary>
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
#if TEAM_ENV
        BlockDragHandler dragTeam = block.GetComponent<BlockDragHandler>();
        if (dragTeam != null && dragTeam.isTemplate)
            return;
#else
        Drag dragCustom = block.GetComponent<Drag>();
        if (dragCustom != null && dragCustom.isTemplate)
            return;
#endif

        if (audioSource != null && trashClip != null)
            audioSource.PlayOneShot(trashClip);

        Destroy(block);
    }
}