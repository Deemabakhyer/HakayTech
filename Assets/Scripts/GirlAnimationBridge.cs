using UnityEngine;

public class GirlAnimationBridge : MonoBehaviour
{
    // اسحبي أوبجكت المانجر هنا في Inspector الطفلة
    public VariableManager manager;

    // هذه الدالة التي ستظهر لكِ في الأنميشن ومعها خيار الـ Int
    public void StartShellInteraction(int shellID)
    {
        if (manager != null) manager.StartShellInteraction(shellID);
    }

    // دالة فتح الصندوق
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
