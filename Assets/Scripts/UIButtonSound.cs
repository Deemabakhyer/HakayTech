using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public enum ButtonSoundType
    {
        NormalButton,
        CloseButton
    }

    public ButtonSoundType soundType = ButtonSoundType.NormalButton;

    private void Awake()
    {
        Button button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (GameUISoundManager.Instance == null)
            return;

        if (soundType == ButtonSoundType.CloseButton)
            GameUISoundManager.Instance.PlayCloseClick();
        else
            GameUISoundManager.Instance.PlayButtonClick();
    }
}