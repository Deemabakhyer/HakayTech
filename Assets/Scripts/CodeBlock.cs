using UnityEngine;
using TMPro;

public class CodeBlock : MonoBehaviour
{
    [Header("Block Info")]
    public string blockName;

    [Header("UI Reference")]
    public TMP_InputField iterationInput;

    public string GetInputValue()
    {
        if (iterationInput != null)
        {
            return iterationInput.text;
        }
        return "";
    }

    public void ClearInputValue()
    {
        if (iterationInput != null)
        {
            iterationInput.text = "";
        }
    }

    public void ResetToOriginalPosition()
    {
        Debug.Log("[CodeBlock] Resetting to original position: " + blockName);
    }
}
