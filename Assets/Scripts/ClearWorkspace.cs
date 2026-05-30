using UnityEngine;
using UnityEngine.UI;

public class ClearWorkspace : MonoBehaviour
{
    [Header("References")]
    public Transform solutionSheet;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (solutionSheet == null)
        {
            GameObject sheet = GameObject.Find("solution_sheet");
            if (sheet != null) solutionSheet = sheet.transform;
        }

        Button btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(DeleteAllBlocks);
    }

    public void DeleteAllBlocks()
    {
        if (solutionSheet == null) return;

        // Play sound immediately
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();

        // Release all SnapSlots anywhere inside the solution sheet first,
        // so no slot holds a stale reference to a block we're about to destroy.
        foreach (SnapSlot slot in solutionSheet.GetComponentsInChildren<SnapSlot>())
            slot.ReleaseBlock();

        // Destroy every direct child that is a DraggableBlock.
        // Iterate backwards so the index stays valid as children are removed.
        int deletedCount = 0;
        for (int i = solutionSheet.childCount - 1; i >= 0; i--)
        {
            GameObject child = solutionSheet.GetChild(i).gameObject;
            if (child.GetComponent<DraggableBlock>() != null)
            {
                Destroy(child);
                deletedCount++;
            }
        }

        if (deletedCount > 0)
            Debug.Log($"Workspace cleared! Destroyed {deletedCount} root blocks.");
    }
}