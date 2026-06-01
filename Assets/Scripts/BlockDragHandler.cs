using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string blockID;
    public bool isTemplate = true;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private BlockDragHandler activeBlock;
    private Vector2 pointerOffset;

    private Camera UICamera
    {
        get
        {
            if (canvas == null) return null;
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
    }

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (isTemplate)
        {
            GameObject copy = Instantiate(gameObject, canvas.transform);
            copy.name = gameObject.name + "_Copy";

            activeBlock = copy.GetComponent<BlockDragHandler>();
            activeBlock.isTemplate = false;
            activeBlock.blockID = blockID;

            RectTransform copyRect = copy.GetComponent<RectTransform>();
            copyRect.position = rectTransform.position;

            CanvasGroup copyGroup = copy.GetComponent<CanvasGroup>();
            if (copyGroup == null)
                copyGroup = copy.AddComponent<CanvasGroup>();

            copyGroup.blocksRaycasts = false;

            SetOffsetFromCanvas(copyRect, eventData);
        }
        else
        {
            activeBlock = this;

            Vector3 worldPosition = rectTransform.position;

            transform.SetParent(canvas.transform, true);
            rectTransform.position = worldPosition;

            canvasGroup.blocksRaycasts = false;

            SetOffsetFromCanvas(rectTransform, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeBlock == null)
            return;

        RectTransform activeRect = activeBlock.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            UICamera,
            out Vector2 pointerLocalPosition
        );

        activeRect.anchoredPosition = pointerLocalPosition - pointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (activeBlock == null)
            return;

        CanvasGroup activeGroup = activeBlock.GetComponent<CanvasGroup>();
        if (activeGroup != null)
            activeGroup.blocksRaycasts = true;

        if (!activeBlock.isTemplate && TrashDropArea.Instance != null)
        {
            if (TrashDropArea.Instance.IsPointerInside(eventData.position))
            {
                TrashDropArea.Instance.DeleteBlock(activeBlock.gameObject);
                activeBlock = null;
                return;
            }
        }

        if (DropBlocksArea.Instance != null)
        {
            DropBlocksArea.Instance.AcceptBlock(
                activeBlock.gameObject,
                activeBlock.blockID
            );
        }

        activeBlock = null;
    }

    private void SetOffsetFromCanvas(RectTransform targetRect, PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            UICamera,
            out Vector2 pointerLocalPosition
        );

        pointerOffset = pointerLocalPosition - targetRect.anchoredPosition;
    }
}