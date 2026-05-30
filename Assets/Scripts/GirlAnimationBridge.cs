using UnityEngine;

public class GirlAnimationBridge : MonoBehaviour
{
    public VariableManager manager;

    public void StartShellInteraction(int shellID)
    {
        if (manager != null) manager.StartShellInteraction(shellID);
    }

    public void PlayBoxOpenEffect()
    {
        if (manager != null) manager.PlayBoxOpenEffect();
    }

    public void MoveShell(int id)
    {
        manager.MoveShellToBox(id);
    }


    public void PlayCelebrationSound()
    {
        if (manager != null) manager.PlayCelebrationSound();
    }

}
