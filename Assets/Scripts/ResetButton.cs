using UnityEngine;

public class ResetButton : MonoBehaviour
{
    // If you are using a Sprite with a Collider
    private void OnMouseDown()
    {
        ExecuteReset();
    }

    // If you are using a UI Button, link this function to the OnClick event
    public void ExecuteReset()
    {
        // Find all blocks in the scene
        CodingBlock[] allBlocks = Object.FindObjectsByType<CodingBlock>(FindObjectsSortMode.None);

        foreach (CodingBlock block in allBlocks)
        {
            // Move back to the saved start position
            block.transform.position = block.startPosition;

            // Tell the block it is no longer in the solution
            block.ResetBlockStatus();
        }

        Debug.Log("All blocks reset to original positions.");
    }
}