using UnityEngine;
using UnityEngine.EventSystems;

public class CodingBlock : MonoBehaviour, IEndDragHandler
{
    public string blockAction = "BoilWater";
    [SerializeField] private bool isInSolution = false;

    public void OnEndDrag(PointerEventData eventData)
    {
        // Creates a tiny circle at the block's position to see what it's touching
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);

        bool foundArea = false;
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("SolutionArea"))
            {
                foundArea = true;
                break;
            }
        }

        isInSolution = foundArea;
        Debug.Log($"{blockAction} is in solution: {isInSolution}");
    }

    public bool IsInSolution() => isInSolution;
}