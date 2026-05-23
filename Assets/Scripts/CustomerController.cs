using UnityEngine;
using System.Collections;

public class CustomerController : MonoBehaviour
{
    private Animator animator;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public Transform stopPosition; // Create an empty GameObject where you want him to stop

    [Header("UI Elements")]
    public GameObject textBubble; // Drag your canvas text bubble UI here

    private bool isMovingToSpot = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (textBubble != null) textBubble.SetActive(false);
    }

    void Update()
    {
        // Move customer forward until he reaches the stop position
        if (isMovingToSpot && stopPosition != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, stopPosition.position, walkSpeed * Time.deltaTime);

            // Check if he arrived
            if (Vector3.Distance(transform.position, stopPosition.position) < 0.1f)
            {
                ArrivedAtSpot();
            }
        }
    }

    void ArrivedAtSpot()
    {
        isMovingToSpot = false;
        animator.SetTrigger("StopWalking"); // Changes animation to 'Standing'

        // Show the dialogue text bubble
        if (textBubble != null)
        {
            textBubble.SetActive(true);
        }
    }

    // Call this function from your Submit Button script/manager
    public void CheckPlayerSubmission(bool isCorrect)
    {
        if (isCorrect)
        {
            // Hide text bubble, play happy dance, then walk away
            if (textBubble != null) textBubble.SetActive(false);

            animator.SetTrigger("IsHappy");

            // Optional: If you want him to walk away after being happy, 
            // you can trigger a "ResumeWalking" state using a Coroutine or StateMachineBehaviour
        }
        else
        {
            // Incorrect: Do nothing. Customer stays in "Standing" (Idle) state.
            Debug.Log("Wrong solution! Customer is unimpressed.");
        }
    }
}