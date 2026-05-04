using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDragScaleEffect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public Vector3 normalScale = Vector3.one;
    public Vector3 dragScale = new Vector3(1.08f, 1.08f, 1f);

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        transform.localScale = normalScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.localScale = dragScale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localScale = normalScale;
    }
}