using UnityEngine;
using UnityEngine.EventSystems;

public class ConditionSlot : MonoBehaviour, IDropHandler
{
    public bool isOccupied = false;
    public GameObject attachedBlock = null;

    public void OnDrop(PointerEventData eventData)
    {
        // Check if something is being dragged
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        // Check if it's an oval/condition block and slot is empty
        if (droppedObj.CompareTag("ConditionBlock") && !isOccupied)
        {
            SnapBlock(droppedObj);
        }
    }

    private void SnapBlock(GameObject block)
    {
        isOccupied = true;
        attachedBlock = block;

        // Parent it to this slot so it moves with the IF block
        block.transform.SetParent(this.transform);

        // Center it perfectly in the slot
        RectTransform rt = block.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
    }
}