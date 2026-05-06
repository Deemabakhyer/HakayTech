using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
public class DragBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Block Data")]
    public string blockID;
    public bool isTemplate = true;

    [Header("Visual")]
    public RectTransform visualToScale;

    [Header("Audio")]
    public AudioSource blockAudio;
    public AudioClip dragClip;

    public static DragBlock CurrentDragged;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private LayoutElement layoutElement;
    private Image blockImage;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalPosition;

    private bool droppedInValidArea = false;
    private bool createdFromTemplate = false;

    private GameObject activeClone;

    private Sprite originalTemplateSprite;
    private Color originalTemplateColor = Color.white;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        layoutElement = GetComponent<LayoutElement>();
        blockImage = GetComponent<Image>();

        if (visualToScale == null)
            visualToScale = rectTransform;

        if (blockImage != null)
        {
            originalTemplateSprite = blockImage.sprite;
            originalTemplateColor = blockImage.color;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        PlayBlockSound();

        if (isTemplate)
        {
            RestoreTemplateVisual();

            activeClone = Instantiate(gameObject, canvas.transform);

            DragBlock clone = activeClone.GetComponent<DragBlock>();
            clone.isTemplate = false;
            clone.createdFromTemplate = true;
            clone.droppedInValidArea = false;

            clone.blockAudio = blockAudio;
            clone.dragClip = dragClip;

            clone.CopyTemplateVisualFrom(this);

            RectTransform cloneRect = activeClone.GetComponent<RectTransform>();
            cloneRect.position = rectTransform.position;

            clone.BeginRealDrag();
            CurrentDragged = clone;
        }
        else
        {
            droppedInValidArea = false;
            BeginRealDrag();
            CurrentDragged = this;
        }
    }

    private void PlayBlockSound()
    {
        if (blockAudio != null && dragClip != null)
            blockAudio.PlayOneShot(dragClip);
    }

    private void BeginRealDrag()
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalPosition = rectTransform.position;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.95f;

        if (layoutElement != null)
            layoutElement.ignoreLayout = true;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        if (visualToScale != null)
            visualToScale.localScale = Vector3.one * 1.12f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CurrentDragged == null) return;

        RectTransform target = CurrentDragged.rectTransform;
        target.position += (Vector3)eventData.delta / CurrentDragged.canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CurrentDragged == null) return;

        CurrentDragged.EndRealDrag();

        CurrentDragged = null;
        activeClone = null;

        if (isTemplate)
            RestoreTemplateVisual();
    }

    private void EndRealDrag()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (visualToScale != null)
            visualToScale.localScale = Vector3.one;

        if (!droppedInValidArea)
        {
            if (createdFromTemplate)
            {
                Destroy(gameObject);
                return;
            }

            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.position = originalPosition;

            if (layoutElement != null)
                layoutElement.ignoreLayout = false;

            Canvas.ForceUpdateCanvases();
            return;
        }

        if (layoutElement != null)
            layoutElement.ignoreLayout = false;

        Canvas.ForceUpdateCanvases();
    }

    public void MarkAsDropped(Transform newParent, int siblingIndex)
    {
        droppedInValidArea = true;

        transform.SetParent(newParent, false);
        transform.SetSiblingIndex(siblingIndex);
        transform.localScale = Vector3.one;

        if (layoutElement != null)
            layoutElement.ignoreLayout = false;

        Canvas.ForceUpdateCanvases();
    }

    private void RestoreTemplateVisual()
    {
        if (blockImage != null && originalTemplateSprite != null)
        {
            blockImage.sprite = originalTemplateSprite;
            blockImage.color = originalTemplateColor;
        }

        BlockEffects effects = GetComponent<BlockEffects>();
        if (effects != null)
            effects.isWrongMarked = false;
    }

    private void CopyTemplateVisualFrom(DragBlock sourceTemplate)
    {
        if (sourceTemplate == null) return;

        Image cloneImage = GetComponent<Image>();
        if (cloneImage != null)
        {
            cloneImage.sprite = sourceTemplate.originalTemplateSprite;
            cloneImage.color = sourceTemplate.originalTemplateColor;
        }

        BlockEffects effects = GetComponent<BlockEffects>();
        if (effects != null)
        {
            effects.blockImage = cloneImage;
            effects.normalSprite = sourceTemplate.originalTemplateSprite;
            effects.isWrongMarked = false;
            effects.ResetBlock();
        }
    }
}