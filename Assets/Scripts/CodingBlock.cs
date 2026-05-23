using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CodingBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform solutionSheet;
    private AudioSource audioSource;
    private Canvas rootCanvas;

    [Header("Settings")]
    public bool isTemplate = false;
    public float snapDistance = 60f;

    [Header("Animation")]
    public string animationTriggerName; // Set this in Inspector e.g. "Pour"
    public Animator blockAnimator;      // Drag the Animator component here

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        solutionSheet = GameObject.Find("solution_sheet").transform;
        audioSource = GetComponent<AudioSource>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
    }

    // Called by SubmitManager when it's this block's turn to play
    public IEnumerator PlayAnimation()
    {
        if (blockAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            blockAnimator.SetTrigger(animationTriggerName);

            // Wait for the animation to start
            yield return null;

            // Wait for the animation to finish
            AnimatorStateInfo stateInfo = blockAnimator.GetCurrentAnimatorStateInfo(0);
            while (blockAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f
                   || blockAnimator.IsInTransition(0))
            {
                yield return null;
            }
        }
        else
        {
            // No animation assigned, just wait a moment
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isTemplate)
        {
            GameObject clone = Instantiate(gameObject, transform.parent);
            clone.transform.position = transform.position;
            CodingBlock cloneScript = clone.GetComponent<CodingBlock>();
            cloneScript.isTemplate = false;
            eventData.pointerDrag = clone;
            cloneScript.PrepareForDrag();
        }
        else
        {
            PrepareForDrag();
        }
    }

    public void PrepareForDrag()
    {
        transform.SetParent(rootCanvas.transform);
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint
        );
        transform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        bool snapped = AttemptSnap();

        if (snapped)
        {
            PlayDropSound();
            return;
        }

        if (eventData.pointerCurrentRaycast.gameObject != null &&
            eventData.pointerCurrentRaycast.gameObject.name == "solution_sheet")
        {
            transform.SetParent(solutionSheet);
            PlayDropSound();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlayDropSound()
    {
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }

    private bool AttemptSnap()
    {
        CodingBlock[] allBlocks = FindObjectsOfType<CodingBlock>();

        CodingBlock bestTarget = null;
        float closestDist = float.MaxValue;

        foreach (CodingBlock other in allBlocks)
        {
            if (other == this || IsChildOf(other.transform) || other.isTemplate) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);

            if (dist < snapDistance && dist < closestDist)
            {
                closestDist = dist;
                bestTarget = other;
            }
        }

        if (bestTarget != null)
        {
            transform.SetParent(bestTarget.transform);
            rectTransform.localScale = Vector3.one;
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.anchoredPosition = new Vector2(0, -50f);
            return true;
        }

        return false;
    }

    private bool IsChildOf(Transform target)
    {
        Transform current = target.parent;
        while (current != null)
        {
            if (current == transform) return true;
            current = current.parent;
        }
        return false;
    }

    [Header("Audio")]
    public AudioClip actionSound; // Drag the specific sound (e.g., water bubbling) here

    public void PlayActionSound()
    {
        if (audioSource != null && actionSound != null)
        {
            // PlayOneShot is best because it doesn't interrupt other sounds
            audioSource.PlayOneShot(actionSound);
        }
    }
}

