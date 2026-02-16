using UnityEngine;
using TMPro;
using System.Collections.Generic; // Required for using Lists

public class LoopBlockLogic : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField iterationInput;

    [Header("Settings")]
    public int correctNumber = 7;

    public void EnableInput()
    {
        if (iterationInput != null)
        {
            iterationInput.interactable = true;
        }
    }

    public bool IsInputCorrect()
    {
        if (iterationInput == null || string.IsNullOrEmpty(iterationInput.text))
        {
            Debug.LogWarning("Input Field is empty or not assigned!");
            return false;
        }
        return iterationInput.text.Trim() == correctNumber.ToString();
    }

    /// <summary>
    /// Checks if the blocks inside the loop are in the correct sequence:
    /// 1. Block2 (بسم الله والله أكبر)
    /// 2. Block3 (طواف شوط كامل)
    /// </summary>
    public bool IsSequenceCorrect()
    {
        if (!IsInputCorrect()) return false;

        // الحصول على جميع البلوكات حتى لو كانت داخل بعضها (Nested)
        LoopBlockLogic[] allSubLogics = GetComponentsInChildren<LoopBlockLogic>();
        List<Transform> sortedBlocks = new List<Transform>();

        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            // نتأكد أننا نأخذ البلوكات فقط ولا نأخذ اللوب الرئيسي نفسه
            if (child != this.transform && child.name.Contains("block"))
            {
                // نتحقق أن الكائن لديه Collider أو اسم محدد لنتجنب أخذ نقاط الـ Snap
                if (child.name.Contains("(Clone)"))
                {
                    sortedBlocks.Add(child);
                }
            }
        }

        // ترتيب البلوكات بناءً على موقعها في العالم (Y) من الأعلى للأسفل
        sortedBlocks.Sort((a, b) => b.position.y.CompareTo(a.position.y));

        if (sortedBlocks.Count >= 2)
        {
            // التحقق من أن الأعلى هو block2 (طف شوطاً) والأسفل هو block3 (الذكر)
            bool isOrderCorrect = sortedBlocks[0].name.ToLower().Contains("block2") &&
                                 sortedBlocks[1].name.ToLower().Contains("block3");

            return isOrderCorrect;
        }

        return false;
    }
}