using UnityEngine;
using UnityEngine.UI;

public class ClearWorkspace : MonoBehaviour
{
    [Header("References")]
    public Transform solutionSheet;

    private AudioSource audioSource;

    private void Awake()
    {
        // Get the AudioSource on the delete button
        audioSource = GetComponent<AudioSource>();

        if (solutionSheet == null)
        {
            GameObject sheet = GameObject.Find("solution_sheet");
            if (sheet != null) solutionSheet = sheet.transform;
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(DeleteAllBlocks);
        }
    }

    public void DeleteAllBlocks()
    {
        // 1. Play the sound immediately when clicked
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // 2. Logic to clear the blocks
        int deletedCount = 0;
        for (int i = solutionSheet.childCount - 1; i >= 0; i--)
        {
            GameObject child = solutionSheet.GetChild(i).gameObject;

            if (child.GetComponent<CodingBlock>() != null)
            {
                Destroy(child);
                deletedCount++;
            }
        }

        // Only log if we actually cleared something
        if (deletedCount > 0) Debug.Log("Workspace cleared!");
    }
}