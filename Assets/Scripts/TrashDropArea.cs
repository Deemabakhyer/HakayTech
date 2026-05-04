using UnityEngine;
using UnityEngine.EventSystems;

public class TrashDropArea : MonoBehaviour, IDropHandler
{
    [Header("References")]
    public BlockChecker blockChecker;

    [Header("Audio")]
    public AudioSource trashAudio;
    public AudioClip trashClip;

    public void OnDrop(PointerEventData eventData)
    {
        if (DragBlock.CurrentDragged == null) return;

        DragBlock dragged = DragBlock.CurrentDragged;
        if (dragged == null) return;
        if (dragged.isTemplate) return;

        // صوت الحذف
        if (trashAudio != null && trashClip != null)
        {
            trashAudio.PlayOneShot(trashClip);
        }

        // حذف البلوك
        Destroy(dragged.gameObject);

        // تحديث الحالة
        if (blockChecker != null)
            blockChecker.EvaluateLiveState();
    }
}