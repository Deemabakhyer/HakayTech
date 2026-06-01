using UnityEngine;

public class BoyReactionTrigger : MonoBehaviour
{
    public BoyOutsideController boyController;

    public void ShowBoyShocked()
    {
        if (boyController != null)
            boyController.ShowState(2);
    }

    public void ShowBoyWelcome()
    {
        if (boyController != null)
            boyController.ShowState(4);
    }

    public void ShowBoyReady()
    {
        if (boyController != null)
            boyController.ShowState(5);
    }
}