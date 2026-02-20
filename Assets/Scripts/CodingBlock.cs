using UnityEngine;
using UnityEngine.EventSystems;

public class CodingBlock : MonoBehaviour, IEndDragHandler
{
    public string blockAction = "BoilWater"; // Set this in the Inspector for each block
    private bool isInSolution = false;

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("SolutionArea"))
        {
            isInSolution = true;
            Debug.Log(blockAction + " added to sequence.");
        }
        else
        {
            isInSolution = false;
        }
    }

    public bool IsInSolution() => isInSolution;
}