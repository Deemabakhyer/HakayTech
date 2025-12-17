using UnityEngine;

public class Drag : MonoBehaviour
{
    [Header("Block Type")]
    public bool isTemplate = false;
    public static Transform solutionSheet;

    private Collider2D col;
    private Vector3 startPos;
    private Camera cam;
    private BlockSnap snap;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        cam = Camera.main;
        snap = GetComponent<BlockSnap>();

        if (solutionSheet == null)
        {
            GameObject sheet = GameObject.Find("SolutionSheet");
            if (sheet != null)
                solutionSheet = sheet.transform;
        }

    }

    private void OnMouseDown()
    {
        if (isTemplate)
        {
            GameObject clone = Instantiate(gameObject);
            clone.transform.SetParent(null, true);
            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;
            clone.transform.localScale = transform.lossyScale;

            Drag drag = clone.GetComponent<Drag>();
            drag.isTemplate = false;

            drag.BeginDrag();
            return;
        }

        // Non-template block → drag itself
        BeginDrag();
    }


    public void BeginDrag()
    {
        startPos = transform.position;

        // Detach from parent when dragging (important for chains)
        transform.SetParent(null);
    }

    private void OnMouseDrag()
    {
        if (isTemplate) return;

        transform.position = GetMouseWorldPos();
    }

    private void OnMouseUp()
    {
        if (isTemplate) return;

        bool snapped = false;

        // Try snapping to another block
        if (snap != null)
        {
            snapped = snap.TrySnap();
        }

        // If not snapped, try drop areas
        if (!snapped)
        {
            col.enabled = false;
            Collider2D hit = Physics2D.OverlapPoint(GetMouseWorldPos());
            col.enabled = true;

            if (hit != null && hit.TryGetComponent(out DropArea drop))
            {
                drop.OnDrop(this);
            }
            else
            {
                transform.position = startPos;
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 p = cam.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }
}
