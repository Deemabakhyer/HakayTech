// DraggableBlock.cs
// Attach to every block prefab (Condition, Number, IfElse).
//
// CLONING STRATEGY:
// Unity's EventSystem does NOT reliably re-route OnDrag/OnEndDrag after swapping
// eventData.pointerDrag mid-flight. Instead, the template acts as a pure proxy:
// it spawns a clone on BeginDrag, then forwards every subsequent OnDrag /
// OnEndDrag call directly to the clone's handlers. The template itself never
// moves. This guarantees the clone receives the full drag lifecycle.

using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Block Identity")]
    public BlockType2 blockType;
    public BlockIdentity blockIdentity; 
    public bool isTemplate = false;

    [Header("Snap Settings")]
    public float snapRadius = 70f;
    public float solutionRadius = 200f;

    [Header("Audio")]
    public AudioClip dropSound;             // Assign in prefab Inspector
    private AudioSource audioSource;

    // ── Runtime refs ─────────────────────────────────────────────────────────

    [HideInInspector] public RectTransform RectTransform;
    [HideInInspector] public SnapSlot CurrentSlot;

    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Transform solutionSheet;

    // When this block IS a template, activeClone is the block currently being
    // dragged on its behalf. Forwarding stops once the drag ends.
    private DraggableBlock activeClone;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        RefreshCanvasRef();

        GameObject sol = GameObject.Find("solution_sheet");
        if (sol != null) solutionSheet = sol.transform;
    }

    private void RefreshCanvasRef()
    {
        Canvas c = GetComponentInParent<Canvas>();
        if (c != null) rootCanvas = c.rootCanvas;
    }

    // ── IBeginDragHandler ─────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) RefreshCanvasRef();
        if (rootCanvas == null) return;

        if (isTemplate)
        {
            // Spawn a clone and keep a reference to forward drag events to it.
            activeClone = SpawnClone();
            if (activeClone != null)
                activeClone.PrepareForDrag(rootCanvas);
            return;
        }

        // Non-template: detach from any slot and start dragging self.
        if (CurrentSlot != null)
        {
            CurrentSlot.ReleaseBlock();
            CurrentSlot = null;
        }

        PrepareForDrag(rootCanvas);
    }

    // ── IDragHandler ──────────────────────────────────────────────────────────

    public void OnDrag(PointerEventData eventData)
    {
        // Template proxies drag events to its active clone.
        if (isTemplate)
        {
            activeClone?.ReceiveDrag(eventData);
            return;
        }

        ReceiveDrag(eventData);
    }

    // ── IEndDragHandler ───────────────────────────────────────────────────────

    public void OnEndDrag(PointerEventData eventData)
    {
        // Template proxies end-drag to its active clone, then clears the ref.
        if (isTemplate)
        {
            activeClone?.ReceiveEndDrag(eventData);
            activeClone = null;
            return;
        }

        ReceiveEndDrag(eventData);
    }

    // ── Core drag methods (called directly on non-templates / via proxy on clones) ──

    /// <summary>Lift this block to the root canvas layer and make it semi-transparent.</summary>
    public void PrepareForDrag(Canvas canvas)
    {
        rootCanvas = canvas;
        solutionSheet = solutionSheet ?? GameObject.Find("solution_sheet")?.transform;

        transform.SetParent(rootCanvas.transform);

        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    public void ReceiveDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint
        );
        transform.position = worldPoint;
    }

    public void ReceiveEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 1. Try snapping into a compatible SnapSlot.
        SnapSlot best = FindBestSnapSlot();
        if (best != null)
        {
            PlayDropSound();
            best.AcceptBlock(this);
            return;
        }

        // 2. Try landing freely on the solution sheet.
        if (IsNearSolutionSheet(eventData))
        {
            PlayDropSound();
            transform.SetParent(solutionSheet);
            ResetLocalTransform();
            return;
        }

        // 3. Dropped nowhere useful — destroy.
        Destroy(gameObject);
    }

    // ── Cloning ───────────────────────────────────────────────────────────────

    private DraggableBlock SpawnClone()
    {
        // Instantiate under the same parent so it appears in the palette layer,
        // at the same world position as the template.
        GameObject clone = Instantiate(gameObject, transform.parent);
        clone.transform.position = transform.position;
        clone.name = gameObject.name;   // Strip Unity's "(Clone)" suffix.

        DraggableBlock cloneScript = clone.GetComponent<DraggableBlock>();
        if (cloneScript == null) return null;

        // Mark as a live (non-template) block and inherit resolved scene refs.
        cloneScript.isTemplate = false;
        cloneScript.rootCanvas = rootCanvas;
        cloneScript.blockType = blockType; // Inherits structure
        cloneScript.blockIdentity = blockIdentity;
        cloneScript.solutionSheet = solutionSheet;
        cloneScript.dropSound = dropSound;      // Inherit clip from template prefab.

        return cloneScript;
    }

    // ── Snap helpers ──────────────────────────────────────────────────────────

    private SnapSlot FindBestSnapSlot()
    {
        SnapSlot[] all = FindObjectsByType<SnapSlot>(FindObjectsSortMode.None);
        SnapSlot best = null;
        float bestDist = snapRadius;

        foreach (SnapSlot slot in all)
        {
            if (slot.ownerBlock == this) continue;
            if (!slot.CanAccept(this)) continue;

            float dist = Vector3.Distance(transform.position, slot.WorldCenter);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = slot;
            }
        }

        return best;
    }

    private bool IsNearSolutionSheet(PointerEventData eventData)
    {
        if (solutionSheet == null) return false;

        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            GameObject hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit.name == "solution_sheet" || hit.transform.IsChildOf(solutionSheet))
                return true;
        }

        return Vector3.Distance(transform.position, solutionSheet.position) < solutionRadius;
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private void PlayDropSound()
    {
        if (audioSource != null && dropSound != null)
            audioSource.PlayOneShot(dropSound);
    }

    private void ResetLocalTransform()
    {
        if (RectTransform != null)
        {
            RectTransform.localScale = Vector3.one;
            RectTransform.localEulerAngles = Vector3.zero;
        }
    }
}