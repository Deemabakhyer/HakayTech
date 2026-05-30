using UnityEngine;
using TMPro;
using System.Collections;


/// <summary>
/// Manages the One-Time Password (OTP) verification process.
/// It handles the countdown timer, email masking for privacy, 
/// and branches the logic between user registration and existing user login.
/// </summary>
public class OTPManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text errorText;
    public TMP_Text timerText;
    public UnityEngine.UI.Button resendButton;

    private float timeRemaining = 120f; // 2min
    private bool timerRunning = false;
    private string idToken;
    private string email;
    [Header("UI References")]
    public TMP_Text infoText;

    void Start()
    {
        idToken = PlayerPrefs.GetString("pendingIdToken");
        email = PlayerPrefs.GetString("pendingEmail");

        if (infoText != null && !string.IsNullOrEmpty(email))
        {
            infoText.text = $"ادخل الرمز المرسل الى البريد\n{MaskEmail(email)}";
        }

        StartTimer();
    }


    /// <summary>
    /// Masks the user's email address by hiding middle characters with asterisks.
    /// This enhances user privacy while providing enough context to identify the recipient inbox.
    /// </summary>
    string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts[0].Length <= 2) return email;

        string name = parts[0];
        string maskedName = name.Substring(0, 3) + new string('*', 7) + name.Substring(name.Length - 2);
        return maskedName + "@" + parts[1];
    }

    void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        if (timerText != null)
        {
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (timeRemaining <= 0)
        {
            timerRunning = false;

            if (timerText != null)
            {
                timerText.text = "00:00";
            }
            if (resendButton != null)
            {
                resendButton.interactable = true; 

                TMP_Text buttonText = resendButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = "إعادة إرسال";
                    buttonText.color = Color.black; 
                }
            }

            ShowError("انتهى وقت الرمز! اضغط إعادة إرسال");
        }
    }


    [Header("OTP Input Fields")]
    public TMP_InputField[] otpFields;

    string GetOTPCode()
    {
        string code = "";
        foreach (var field in otpFields)
            code += field.text;
        return code;
    }

    public void OnVerifyClicked()
    {
        string otp = GetOTPCode();

        if (otp.Length < 6)
        {
            ShowError("Please enter the complete code!");
            return;
        }

        StartCoroutine(VerifyOTP());
    }

    void StartTimer()
    {
        timeRemaining = 120f; 
        timerRunning = true;

        if (resendButton != null)
        {
            resendButton.interactable = false; 

            TMP_Text buttonText = resendButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "إعادة الإرسال خلال";
                buttonText.color = new Color(0.3f, 0.3f, 0.3f, 0.6f); // تحويله للرمادي الباهت ليفهم الطفل أنه غير نشط حالياً
            }
        }
    }


    /// <summary>
    /// Validates the entered 6-digit code against the locally stored OTP and checks for expiration.
    /// If valid, it determines whether to proceed with creating a new Firestore document or loading an existing profile.
    /// </summary>
    IEnumerator VerifyOTP()
    {
        string enteredOTP = GetOTPCode();
        string savedOTP = PlayerPrefs.GetString("otpCode");
        string expiryStr = PlayerPrefs.GetString("otpExpiry");

        System.DateTime expiry = System.DateTime.Parse(expiryStr);
        if (System.DateTime.UtcNow > expiry)
        {
            ShowError("انتهى وقت الرمز! اضغط إعادة إرسال");
            yield break;
        }
        
        if (enteredOTP == savedOTP)
        {
            Debug.Log("OTP Verified!");
            PlayerPrefs.DeleteKey("otpCode");
            PlayerPrefs.DeleteKey("otpExpiry");

            string mode = PlayerPrefs.GetString("loginMode", "signup");
            Debug.Log("MODE: " + mode);

            if (mode == "login")
                StartCoroutine(LoadUserAndProceed()); 
            else
                StartCoroutine(SaveUserAndProceed()); 
        }
        else
        {
            ShowError("الرمز غير صحيح! حاولي مرة ثانية");
        }

        yield return null;
    }
    /// <summary>
    /// Retrieves the unique User ID using the verified email and fetches the full profile from Firestore.
    /// Facilitates the transition to the main dashboard for returning users.
    /// </summary>
    IEnumerator LoadUserAndProceed()
    {
        string email = PlayerPrefs.GetString("pendingEmail");
        string foundUserId = "";
        yield return StartCoroutine(FirebaseManager.Instance.GetUserIdByEmail(
            email,
            (uid) => foundUserId = uid,
            (error) => ShowError("فشل إيجاد المستخدم: " + error)
        ));

        if (string.IsNullOrEmpty(foundUserId))
        {
            ShowError("المستخدم غير موجود!");
            yield break;
        }

        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(
            foundUserId,
            (user) =>
            {
                Debug.Log("Login successful: " + user.name);
                PlayerPrefs.SetString("currentUserId", foundUserId);
                PlayerPrefs.DeleteKey("pendingEmail");
                PlayerPrefs.DeleteKey("loginMode");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Home_Page");
            },
            (error) => ShowError("فشل تحميل البيانات: " + error)
        ));
    }

    /// <summary>
    /// Compiles all temporary 'pending' data from PlayerPrefs to create a new UserGameData profile.
    /// It automatically assigns an initial "Ai Companion" based on the user's gender and initializes game stats.
    /// </summary>
    IEnumerator SaveUserAndProceed()
    {
        Debug.Log("🔥 SAVING USER NOW");
        string userId = PlayerPrefs.GetString("pendingUserId");
        string gender = PlayerPrefs.GetString("pendingGender");
        string aiCompanion = (gender == "انثى" || gender == "female")
            ? "girl_comp"
            : "boy_comp";
        UserGameData newUser = new UserGameData
        {
            userId = userId,
            name = PlayerPrefs.GetString("pendingName"),
            email = PlayerPrefs.GetString("pendingEmail"),
            age = PlayerPrefs.GetInt("pendingAge"),
            gender = gender,
            grade = PlayerPrefs.GetString("pendingGrade"),
            aiCompanionId = aiCompanion,
            avatar = "avatar1",
            accumulatedCoins = 0,
            earnedBadges = new System.Collections.Generic.List<string>()
        };

        Debug.Log("USER DATA: " + JsonUtility.ToJson(newUser));
        bool saved = false;
        yield return StartCoroutine(FirestoreManager.Instance.SaveUser(
            newUser,
            () => saved = true,
            (error) => ShowError("Failed to save user! " + error)
        ));

        if (saved)
        {
            Debug.Log("✅ USER SAVED SUCCESSFULLY");

            PlayerPrefs.SetString("currentUserId", userId);
            PlayerPrefs.DeleteKey("pendingIdToken");
            PlayerPrefs.DeleteKey("pendingUserId");
            PlayerPrefs.DeleteKey("pendingEmail");
            PlayerPrefs.DeleteKey("pendingName");
            PlayerPrefs.DeleteKey("pendingAge");
            PlayerPrefs.DeleteKey("pendingGender");
            PlayerPrefs.DeleteKey("pendingGrade");

            UnityEngine.SceneManagement.SceneManager.LoadScene("Home_Page");
        }
    }
    /// <summary>
    /// Triggers a new OTP generation and delivery request, resetting the UI countdown timer.
    /// </summary>
    public void OnResendClicked()
    {
        StartCoroutine(FirebaseManager.Instance.SendOTP(
            email, 
            (response) =>
            {
                Debug.Log("OTP Resent!");
                StartTimer();
                ShowError("");
            },
            (error) => ShowError("Failed to resend OTP!")
        ));
    }

    void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
    }
}