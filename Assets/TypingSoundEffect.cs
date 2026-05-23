using UnityEngine;
using TMPro;

public class TypingSoundEffect : MonoBehaviour
{
    private TMP_InputField inputField;
    private int previousLength;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        previousLength = 0;

        if (inputField != null)
            inputField.onValueChanged.AddListener(OnTextChanged);
    }

    private void OnDisable()
    {
        if (inputField != null)
            inputField.onValueChanged.RemoveListener(OnTextChanged);
    }

    private void OnTextChanged(string text)
    {
        if (text.Length > previousLength)
        {
            if (GameUISoundManager.Instance != null)
                GameUISoundManager.Instance.PlayTyping();
        }

        previousLength = text.Length;
    }
}