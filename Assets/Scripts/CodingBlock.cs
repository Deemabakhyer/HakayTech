using UnityEngine;
using UnityEngine.EventSystems;

public class CodingBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform solutionSheet;
    private AudioSource audioSource; // Reference to the audio component

    [Header("Settings")]
    public bool isTemplate = false; // Check this for blocks inside 'block_sheet'
    public float snapDistance = 100f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        // Find the solution sheet by name so the clone knows where to go
        solutionSheet = GameObject.Find("solution_sheet").transform;

        // Grab the AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isTemplate)
        {
            // 1. Create a copy of this block
            GameObject clone = Instantiate(gameObject, transform.parent);
            CodingBlock cloneScript = clone.GetComponent<CodingBlock>();

            // 2. The clone is no longer a template; it's a real coding block
            cloneScript.isTemplate = false;

            // 3. Hand over the drag focus to the clone
            eventData.pointerDrag = clone;

            // 4. Set the clone's visuals
            cloneScript.PrepareForDrag();
        }
        else
        {
            PrepareForDrag();
        }
    }

    private void PrepareForDrag()
    {
        // Move to top hierarchy so it drags over everything
        transform.SetParent(transform.root);
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // This will now move the clone if the original was a template
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        bool snapped = AttemptSnap();

        if (!AttemptSnap())
        {
            // If dropped on the solution sheet specifically
            if (eventData.pointerCurrentRaycast.gameObject?.name == "solution_sheet")
            {
                transform.SetParent(solutionSheet);
            }
            else
            {
                // If dropped nowhere valid, destroy the clone (or return it if not a clone)
                if (!isTemplate) Destroy(gameObject);
            }
        }

        if (snapped)
        {
            PlayDropSound();
        }
        else
        {
            // If it's dropped on the solution sheet but doesn't snap to a block
            if (eventData.pointerCurrentRaycast.gameObject?.name == "solution_sheet")
            {
                transform.SetParent(GameObject.Find("solution_sheet").transform);
                PlayDropSound();
            }
            else if (!isTemplate)
            {
                Destroy(gameObject);
            }
        }
    }

    private void PlayDropSound()
    {
        // Check if audioSource exists and has a clip assigned
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    private bool AttemptSnap()
    {
        // Find all blocks, including clones
        CodingBlock[] allBlocks = FindObjectsOfType<CodingBlock>();

        // Get the height from the RectTransform
        float blockHeight = rectTransform.rect.height;

        CodingBlock bestTarget = null;
        float closestDist = float.MaxValue;

        foreach (CodingBlock other in allBlocks)
        {
            // 1. Safety Checks: Don't snap to yourself, your own children, or templates
            if (other == this || isChildOf(other.transform) || other.isTemplate) continue;

            // 2. Use World Distance for detection (most reliable for "closeness")
            float dist = Vector3.Distance(transform.position, other.transform.position);

            if (dist < snapDistance && dist < closestDist)
            {
                closestDist = dist;
                bestTarget = other;
            }
        }

        // 3. Perform the Snap if a target was found
        if (bestTarget != null)
        {
            transform.SetParent(bestTarget.transform);

            // We use anchoredPosition because it's relative to the Parent's pivot
            // If Pivot is (0.5, 0.5), (0, -blockHeight) puts this block's center 
            // exactly at the bottom edge of the parent block.
            rectTransform.anchoredPosition = new Vector2(0, -blockHeight);

            // Reset scale to 1 to prevent blocks from shrinking or growing
            rectTransform.localScale = Vector3.one;
            rectTransform.localEulerAngles = Vector3.zero;

            return true;
        }

        return false;
    }

    private bool isChildOf(Transform target)
    {
        Transform current = target.parent;
        while (current != null)
        {
            if (current == transform) return true;
            current = current.parent;
        }
        return false;
    }
}