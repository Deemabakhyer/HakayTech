using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    // EmailJS Config
    private const string EmailJSServiceID = "service_k6yc9qr";
    private const string EmailJSTemplateID = "template_k3aqg9e";
    private const string EmailJSPublicKey = "ktOICyPyTw3b-Dwsf";
    private const string EmailJSUrl = "https://api.emailjs.com/api/v1.0/email/send";


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // Sign Up
    public IEnumerator SignUp(string email, string password,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        var body = new Dictionary<string, object>
        {
            { "email", email },
            { "password", password },
            { "returnSecureToken", true }
        };

        yield return SendRequest(FirebaseConfig.SignUpUrl, body, onSuccess, onError);
    }

    // Send OTP via EmailJS
    public IEnumerator SendOTP(string email,
        System.Action<string> onSuccess, System.Action<string> onError)

    {
        Debug.Log("Sending OTP to: " + email); // أضيفي هذا ✓
        // ولّد كود 6 أرقام
        string otpCode = UnityEngine.Random.Range(100000, 999999).ToString();

        // احفظه مع وقت الانتهاء
        PlayerPrefs.SetString("otpCode", otpCode);
        PlayerPrefs.SetString("otpExpiry",
            System.DateTime.UtcNow.AddMinutes(2).ToString());

        // أرسله عبر EmailJS
        var body = new Dictionary<string, object>
    {
        { "service_id", EmailJSServiceID },
        { "template_id", EmailJSTemplateID },
        { "user_id", EmailJSPublicKey },
        { "template_params", new Dictionary<string, object>
            {
                { "to_email", email },
                { "otp_code", otpCode }
            }
        }
    };

        yield return SendRequest(EmailJSUrl, body, onSuccess, onError);
    }



    // Helper
    IEnumerator SendRequest(string url, Dictionary<string, object> body,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        string json = JsonConvert.SerializeObject(body);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text);
        }
    }

    // Check if email already exists in Firestore
    public IEnumerator CheckEmailExists(string email,
        System.Action<bool> onResult, System.Action<string> onError)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/users";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // تحقق إذا الإيميل موجود في النتائج
                bool exists = request.downloadHandler.text.Contains(email);
                onResult?.Invoke(exists);
            }
            else
            {
                onError?.Invoke(request.downloadHandler.text);
            }
        }
    }


    public IEnumerator LoadUserByEmail(string email,
    System.Action<UserGameData> onSuccess, System.Action<string> onError)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/users";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject
                    <System.Collections.Generic.Dictionary<string, object>>(request.downloadHandler.text);

                // ابحثي عن المستخدم بالإيميل
                string responseText = request.downloadHandler.text;
                if (responseText.Contains(email))
                {
                    // استخرجي البيانات
                    UserGameData user = new UserGameData { email = email };
                    onSuccess?.Invoke(user);
                }
                else
                {
                    onError?.Invoke("User not found!");
                }
            }
            else
            {
                onError?.Invoke(request.downloadHandler.text);
            }
        }
    }




    // Check if email is verified
    public IEnumerator CheckEmailVerified(string idToken,
        System.Action<bool> onResult, System.Action<string> onError)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={FirebaseConfig.ApiKey}";

        var body = new System.Collections.Generic.Dictionary<string, object>
    {
        { "idToken", idToken }
    };

        yield return SendRequest(url, body,
            (response) =>
            {
                bool verified = response.Contains("\"emailVerified\": true");
                onResult?.Invoke(verified);
            },
            (error) => onError?.Invoke(error)
        );
    }

    public IEnumerator GetUserIdByEmail(string email,
    System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/users";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                if (!responseText.Contains(email))
                {
                    onError?.Invoke("Email not found!");
                    yield break;
                }

                // استخرجي الـ userId من الـ document name
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseText);
                if (data.ContainsKey("documents"))
                {
                    var docs = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
                        data["documents"].ToString());

                    foreach (var doc in docs)
                    {
                        string docFields = doc["fields"].ToString();
                        if (docFields.Contains(email))
                        {
                            // استخرجي الـ ID من الـ name
                            string docName = doc["name"].ToString();
                            string userId = docName.Split('/')[^1];
                            onSuccess?.Invoke(userId);
                            yield break;
                        }
                    }
                }
                onError?.Invoke("User not found!");
            }
            else
            {
                onError?.Invoke(request.downloadHandler.text);
            }
        }
    }



}