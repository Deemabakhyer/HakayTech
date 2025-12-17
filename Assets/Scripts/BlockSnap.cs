using UnityEngine;

public class BlockSnap : MonoBehaviour
{
    public Transform topSnap;
    public Transform bottomSnap;

    public float snapDistance = 0.3f;

    private Drag drag;

    private void Awake()
    {
        drag = GetComponent<Drag>();
    }

    public bool TrySnap()
    {
        // Templates should never snap
        if (drag != null && drag.isTemplate)
            return false;

        BlockSnap[] allBlocks = FindObjectsOfType<BlockSnap>();

        foreach (BlockSnap other in allBlocks)
        {
            if (other == this) continue;

            // Ignore template blocks
            if (other.drag != null && other.drag.isTemplate)
                continue;

            float dist = Vector2.Distance(
                bottomSnap.position,
                other.topSnap.position
            );

            if (dist <= snapDistance)
            {
                SnapTo(other);
                return true;
            }
        }

        return false;
    }

    private void SnapTo(BlockSnap target)
    {
        // Parent to target
        transform.SetParent(target.transform);

        // Align bottom snap to target top snap
        Vector3 offset = transform.position - bottomSnap.position;
        transform.position = target.topSnap.position + offset;
    }
}
