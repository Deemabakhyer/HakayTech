using UnityEngine;

public class BlockSnap : MonoBehaviour
{
    public Transform topSnap;
    public Transform bottomSnap;

    public float snapDistance = 30f;

    private DragBlock drag;

    private void Awake()
    {
        drag = GetComponent<DragBlock>();
    }

    public bool TrySnap()
    {
        if (drag != null && drag.isTemplate)
            return false;

        BlockSnap[] allBlocks = FindObjectsByType<BlockSnap>(FindObjectsSortMode.None);

        foreach (BlockSnap other in allBlocks)
        {
            if (other == this) continue;

            if (other.drag != null && other.drag.isTemplate)
                continue;

            if (other.topSnap == null || bottomSnap == null)
                continue;

            float dist = Vector2.Distance(bottomSnap.position, other.topSnap.position);

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
        if (target == null || target.topSnap == null || bottomSnap == null)
            return;

        transform.SetParent(target.transform.parent);

        Vector3 offset = transform.position - bottomSnap.position;
        transform.position = target.topSnap.position + offset;
    }
}