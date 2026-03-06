using UnityEngine;
using UnityEngine.EventSystems;

public class CodingBlock : MonoBehaviour, IEndDragHandler, IDragHandler, IBeginDragHandler
{
    public string blockAction;
    [SerializeField] private bool isInSolution = false;

    // This is the variable the ResetButton was missing!
    [HideInInspector] public Vector3 startPosition;

    private Transform solutionAreaTransform;

    void Start()
    {
        // Record the original position as soon as the game starts
        startPosition = transform.position;

        GameObject area = GameObject.FindWithTag("SolutionArea");
        if (area != null) solutionAreaTransform = area.transform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (solutionAreaTransform == null) return;

        float distance = Vector2.Distance(transform.position, solutionAreaTransform.position);

        if (distance < 2.0f)
        {
            isInSolution = true;
            Debug.Log($"<color=cyan>{blockAction}</color> is PLACED correctly!");
        }
        else
        {
            isInSolution = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    // This allows the ResetButton to "turn off" the block
    public void ResetBlockStatus()
    {
        isInSolution = false;
    }

    public bool IsInSolution() => isInSolution;
}