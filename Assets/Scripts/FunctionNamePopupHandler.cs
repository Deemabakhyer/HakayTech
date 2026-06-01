using UnityEngine;
using TMPro;

public class FunctionNamePopupHandler : MonoBehaviour
{
    [Header("Popups")]
    public GameObject currentPopup;
    public GameObject nextPopup;

    [Header("Input")]
    public TMP_InputField nameInputField;

    [Header("Optional Error Message")]
    public GameObject errorMessage;

    public void ConfirmFunctionName()
    {
        string input = nameInputField.text.Trim();

        if (input == "make_areeka" || input == "make_areeka()")
        {
            if (errorMessage != null)
                errorMessage.SetActive(false);

            currentPopup.SetActive(false);

            if (nextPopup != null)
                nextPopup.SetActive(true);
        }
        else
        {
            if (errorMessage != null)
                errorMessage.SetActive(true);

            Debug.Log("Wrong function name. Player typed: " + input);
        }
    }

    public void ClosePopup()
    {
        currentPopup.SetActive(false);
    }
}