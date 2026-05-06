using UnityEngine;

public class SuccessCelebrationTrigger : MonoBehaviour
{
    [Header("Success FX")]
    public GameObject successRoot;
    public DualSuccessBurstFX dualSuccessBurstFX;

    [Header("Completion Popup")]
    public CompletionPopupController completionPopup;

    public void PlaySuccess()
    {
        Debug.Log("SUCCESS CELEBRATION TRIGGERED");

        if (successRoot != null)
        {
            successRoot.SetActive(true);
            successRoot.transform.SetAsLastSibling();
        }

        if (dualSuccessBurstFX != null)
            dualSuccessBurstFX.PlayBurst();

        if (completionPopup != null)
            completionPopup.ShowPopup();
    }
}