using UnityEngine;
using TMPro;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField emailInputField;

    [Header("UI Feedback")]
    public TMP_Text errorText;

    public void OnLoginClicked()
    {
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            ShowError("يرجى إدخال البريد الإلكتروني!");
            return;
        }

        if (!IsValidEmail(emailInputField.text))
        {
            ShowError("يرجى إدخال بريد إلكتروني صحيح!");
            return;
        }

        // ✅ هنا الصح
        PlayerPrefs.SetString("loginMode", "login");
        StartCoroutine(LoginFlow());
    }

    IEnumerator LoginFlow()
    {
   
        string email = emailInputField.text.Trim().ToLower();

        bool emailExists = false;
        yield return StartCoroutine(FirebaseManager.Instance.CheckEmailExists(
            email,
            (exists) => emailExists = exists,
            (error) => ShowError("خطأ في الاتصال!")
        ));

        if (!emailExists)
        {
            ShowError("البريد الإلكتروني غير مسجل!");
            yield break;
        }

        yield return StartCoroutine(FirebaseManager.Instance.SendOTP(
            email,
            (response) =>
            {
                Debug.Log("OTP sent for login!");
                PlayerPrefs.SetString("pendingEmail", email);
                PlayerPrefs.SetString("loginMode", "login");
                UnityEngine.SceneManagement.SceneManager.LoadScene("OTP");
            },
            (error) => ShowError("فشل إرسال الرمز: " + error)
        ));
    }

    bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
        Debug.LogWarning(message);
    }


}