// SnapSlot.cs
// Attach to each slot GameObject inside IF/ELSE blocks.
// - Oval slot  → set acceptedType = Condition
// - Body slot  → set acceptedType = Number

using UnityEngine;
using UnityEngine.UI;

public class SnapSlot : MonoBehaviour
{
    [Header("Slot Config")]
    public BlockType2 acceptedType;          // What block type this slot accepts
    public float snapRadius = 70f;          // World-space distance to trigger a snap

    [HideInInspector] public DraggableBlock snappedBlock = null;  // Currently held block
    [HideInInspector] public DraggableBlock ownerBlock = null;  // The IF/ELSE block this slot belongs to

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Auto-find owner: walk up until we hit a DraggableBlock
        Transform t = transform.parent;
        while (t != null)
        {
            ownerBlock = t.GetComponent<DraggableBlock>();
            if (ownerBlock != null) break;
            t = t.parent;
        }
    }

    // ── Called by DraggableBlock.OnEndDrag ──────────────────────────────────

    /// <summary>Returns true if a block of the given type is close enough to snap here.</summary>
    public bool CanAccept(DraggableBlock block)
    {
        if (snappedBlock != null) return false;                   // Already occupied
        if (block.blockType != acceptedType) return false;        // Wrong type
        if (block == ownerBlock) return false;                    // Can't snap to itself

        float dist = Vector3.Distance(block.transform.position, transform.position);
        return dist <= snapRadius;
    }

    /// <summary>Snap the block into this slot.</summary>
    public void AcceptBlock(DraggableBlock block)
    {
        snappedBlock = block;
        block.CurrentSlot = this;

        // Parent the block to this slot so it moves with the IF/ELSE container
        block.transform.SetParent(transform);
        block.RectTransform.anchoredPosition = Vector2.zero;
        block.RectTransform.localScale = Vector3.one;
        block.RectTransform.localEulerAngles = Vector3.zero;
    }

    /// <summary>Release the currently snapped block (called when user drags it away).</summary>
    public void ReleaseBlock()
    {
        snappedBlock = null;
    }

    /// <summary>World-space center of this slot (used for proximity checks).</summary>
    public Vector3 WorldCenter => rectTransform.position;
}