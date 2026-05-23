using UnityEngine;
using UnityEngine.EventSystems;

public class BodySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        // Check if it's a number block or statement
        if (droppedObj.CompareTag("NumberBlock"))
        {
            // Simply parent it; the Vertical Layout Group handles the rest!
            droppedObj.transform.SetParent(this.transform);
        }
    }
}