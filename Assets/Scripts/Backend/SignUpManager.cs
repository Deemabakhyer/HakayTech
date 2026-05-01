using UnityEngine;
using TMPro;
using System.Collections;

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




    IEnumerator SignUpFlow()
    {
        string email = emailInputField.text;
        string password = System.Guid.NewGuid().ToString(); // temporary password

        // 2. Check email uniqueness
        bool emailExists = false;
        yield return StartCoroutine(FirebaseManager.Instance.CheckEmailExists(
            email,
            (exists) => emailExists = exists,
            (error) => ShowError("خطأ في الاتصال!")
        ));

        if (emailExists)
        {
            ShowError("ذا البريد الإلكتروني مسجل مسبقاً");
            yield break;
        }

        // 3. Create Firebase Auth account
        string idToken = "";
        string userId = "";

        yield return StartCoroutine(FirebaseManager.Instance.SignUp(
            email, password,
            (response) => {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject
                    <System.Collections.Generic.Dictionary<string, object>>(response);

                idToken = data["idToken"].ToString();
                userId = data["localId"].ToString(); // ✅ المهم
            },
            (error) => ShowError("فشل التسجيل: " + error)
        ));

        if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(userId)) yield break;

        // خزنيهم
        PlayerPrefs.SetString("pendingIdToken", idToken);
        PlayerPrefs.SetString("pendingUserId", userId);
        PlayerPrefs.SetString("pendingEmail", email);

        if (string.IsNullOrEmpty(idToken)) yield break;

        // 4. Send OTP
        yield return StartCoroutine(FirebaseManager.Instance.SendOTP(
            email,
            (response) => {
                Debug.Log("OTP sent!");

                PlayerPrefs.SetString("pendingName", nameInputField.text);
                PlayerPrefs.SetInt("pendingAge", ageDropdown.value);
                PlayerPrefs.SetString("pendingGender", sexDropdown.options[sexDropdown.value].text);
                PlayerPrefs.SetString("pendingGrade", classDropdown.options[classDropdown.value].text);
                // انتقلي لصفحة OTP
                PlayerPrefs.SetString("pendingEmail", email);
                PlayerPrefs.SetString("pendingIdToken", idToken);
                UnityEngine.SceneManagement.SceneManager.LoadScene("OTP");
            },
            (error) => ShowError("فشل ارسال رمز التحقق!" +error)
        ));
    }

    void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
        Debug.LogWarning(message);
    }



    bool IsValidEmail(string email)
    {
        // تحقق من صياغة الإيميل
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    bool IsValidName(string name)
    {
        // تحقق إن الاسم ثنائي (كلمتين على الأقل)
        string[] parts = name.Trim().Split(' ');
        return parts.Length >= 2 && parts[0].Length > 0 && parts[1].Length > 0;
    }


    public void OnSignUpClicked()
    {
        if (string.IsNullOrEmpty(nameInputField.text) ||
            string.IsNullOrEmpty(emailInputField.text))
        {
            ShowError("يرجى تعبئة جميع الحقول!");
            return;
        }

        if (!IsValidName(nameInputField.text))
        {
            ShowError("يرجى إدخال الاسم الثنائي كاملاً!");
            return;
        }

        if (!IsValidEmail(emailInputField.text))
        {
            ShowError("يرجى إدخال بريد إلكتروني صحيح!");
            return;
        }
        PlayerPrefs.SetString("loginMode", "signup"); // 👈 هنا بالضبط
        StartCoroutine(SignUpFlow());
    }

}