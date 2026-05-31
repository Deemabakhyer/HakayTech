using UnityEngine;
using TMPro;
using System.Collections;
/// <summary>
/// Manages the user registration process, including form validation and account creation.
/// It coordinates with Firebase Authentication for identity management and initiates 
/// the OTP verification flow by caching temporary user data.
/// </summary>
public class SignUpManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    [Header("Dropdowns")]
    public TMP_Dropdown ageDropdown;
    public TMP_Dropdown sexDropdown;
    public TMP_Dropdown classDropdown;
    [Header("UI Feedback")]
    public TMP_Text errorText;
    /// <summary>
    /// Orchestrates the asynchronous registration sequence:
    /// 1. Validates email uniqueness in the database.
    /// 2. Creates a Firebase Auth record with a secure temporary identifier.
    /// 3. Caches profile metadata (Name, Age, Gender) locally before redirecting to OTP verification.
    /// </summary>
    /// 
    IEnumerator SignUpFlow()
    {
        string email = emailInputField.text;
        string password = System.Guid.NewGuid().ToString();
        bool emailExists = false;
        yield return StartCoroutine(FirebaseManager.Instance.CheckEmailExists(
            email,
            (exists) => emailExists = exists,
            (error) => ShowError("خطأ في الاتصال!")
        ));

        if (emailExists)
        {
            ShowError("البريد الإلكتروني مسجل مسبقاً");
            yield break;
        }
        PerformanceLogger.Instance.StartMeasure("OTP_Send");
        yield return StartCoroutine(FirebaseManager.Instance.SendOTP(
            email,
            (response) => {
                PerformanceLogger.Instance.StopMeasure("OTP_Send");
                Debug.Log("OTP sent!");
                PlayerPrefs.SetString("pendingEmail", email);
                PlayerPrefs.SetString("pendingPassword", password);
                PlayerPrefs.SetString("pendingName", nameInputField.text);
                PlayerPrefs.SetInt("pendingAge", ageDropdown.value);
                PlayerPrefs.SetString("pendingGender", sexDropdown.options[sexDropdown.value].text);
                PlayerPrefs.SetString("pendingGrade", classDropdown.options[classDropdown.value].text);
                PlayerPrefs.SetString("loginMode", "signup");

                UnityEngine.SceneManagement.SceneManager.LoadScene("OTP");
            },
            (error) => {
                PerformanceLogger.Instance.StopMeasure("OTP_Send");
                ShowError("فشل ارسال رمز التحقق!" + error);
            }
        ));
    }

    void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
        Debug.LogWarning(message);
    }
    /// <summary>
    /// Validates the structure of the provided email address using standard regex patterns.
    /// </summary>
    bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }
    /// <summary>
    /// Ensures the user has entered at least a first and last name for proper academic record keeping.
    /// </summary>
    bool IsValidName(string name)
    {
        string[] parts = name.Trim().Split(' ');
        return parts.Length >= 2 && parts[0].Length > 0 && parts[1].Length > 0;
    }
    /// <summary>
    /// Validates all UI input fields for completeness and correct formatting.
    /// If validation passes, it sets the session mode to 'signup' and triggers the registration flow.
    /// </summary>
    public void OnSignUpClicked()
    {
        if (string.IsNullOrEmpty(nameInputField.text) ||
            string.IsNullOrEmpty(emailInputField.text))
        {
            ShowError("يرجى تعبئة جميع الحقول");
            return;
        }

        if (!IsValidName(nameInputField.text))
        {
            ShowError("يرجى إدخال الاسم الثنائي كاملا");
            return;
        }

        if (!IsValidEmail(emailInputField.text))
        {
            ShowError("يرجى إدخال بريد إلكتروني صحيح");
            return;
        }
        PlayerPrefs.SetString("loginMode", "signup"); 
        StartCoroutine(SignUpFlow());
    }
}
