using UnityEngine;

public class BlockSnap : MonoBehaviour
{
    [Header("Snapping Points")]
    public Transform topSnap;
    public Transform bottomSnap;
    public Transform innerSnap; // اسحبي نقطة الـ InnerSnap الجديدة هنا في الـ Inspector

    [Header("Settings")]
    public float snapDistance = 0.5f; // زدت القيمة قليلاً لتسهيل الالتصاق للطفل
    private Drag drag;

    [Header("Audio")]
    [Tooltip("Attach the snap sound effect here (e.g., Click or Pop)")]
    public AudioClip snapSound; // اسحبي ملف الصوت هنا في الـ Inspector


    private void Awake()
    {
        drag = GetComponent<Drag>();
    }

    public bool TrySnap()
    {
        if (drag != null && drag.isTemplate) return false;

        BlockSnap[] allBlocks = FindObjectsOfType<BlockSnap>();

        foreach (BlockSnap other in allBlocks)
        {
            if (other == this) continue;
            if (other.drag != null && other.drag.isTemplate) continue;

            // 1. اختبار الالتصاق تحت البلوك (عادي)
            float distToBottom = Vector2.Distance(topSnap.position, other.bottomSnap.position);

            // 2. اختبار الالتصاق داخل اللوب (إذا كان الآخر لديه نقطة داخلية)
            float distToInner = float.MaxValue;
            if (other.innerSnap != null)
            {
                distToInner = Vector2.Distance(topSnap.position, other.innerSnap.position);
            }

            if (distToBottom <= snapDistance)
            {
                SnapTo(other, other.bottomSnap);
                PlaySnapSound();
                return true;
            }
            else if (distToInner <= snapDistance)
            {
                SnapTo(other, other.innerSnap);
                PlaySnapSound();
                return true;
            }
        }
        return false;
    }

    private void SnapTo(BlockSnap target, Transform snapPoint)
    {
        transform.SetParent(target.transform);

        // حساب المسافة الأصلية
        Vector3 offset = transform.position - topSnap.position;

        // إضافة إزاحة بسيطة (يمكنك تعديل هذه القيم لتناسب مقاسات بلوكاتك)
        // x يدفعه لليمين قليلًا، و y يضبط الارتفاع داخل الفتحة
        Vector3 adjustment = new Vector3(0.2f, -0.1f, 0);

        transform.position = snapPoint.position + offset + adjustment;

        // Enable the Arabic input field only when the block is correctly placed in the workspace
        if (TryGetComponent(out LoopBlockLogic loopLogic))
        {
            loopLogic.EnableInput();
        }
    }

    /// <summary>
    /// Plays the snapping sound effect at the block's current position.
    /// </summary>
    private void PlaySnapSound()
    {
        if (snapSound != null)
        {
            // PlayClipAtPoint creates a temporary audio object that destroys itself after finishing
            AudioSource.PlayClipAtPoint(snapSound, Camera.main.transform.position, 1.0f);
        }
    }
}





