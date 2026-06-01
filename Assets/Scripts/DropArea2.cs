using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropArea2 : MonoBehaviour, IDropHandler
{
    public RiyadhBlockChecker blockChecker;

    public void OnDrop(PointerEventData eventData)
    {
        if (DragBlock.CurrentDragged == null) return;

        DragBlock dragged = DragBlock.CurrentDragged;

        Transform firstWrongBlock = FindFirstWrongBlock();
        int targetIndex;

        if (firstWrongBlock != null)
        {
            targetIndex = firstWrongBlock.GetSiblingIndex();
            Destroy(firstWrongBlock.gameObject);
        }
        else
        {
            targetIndex = transform.childCount;
        }

        dragged.MarkAsDropped(transform, targetIndex);

        StartCoroutine(RefreshLayout());
    }

    private Transform FindFirstWrongBlock()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            BlockEffects effects = transform.GetChild(i).GetComponent<BlockEffects>();
            if (effects != null && effects.isWrongMarked)
                return transform.GetChild(i);
        }

        return null;
    }

    IEnumerator RefreshLayout()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        if (blockChecker != null)
            blockChecker.EvaluateLiveState();
    }
}