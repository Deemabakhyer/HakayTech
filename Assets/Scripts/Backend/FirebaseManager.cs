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
    /// <summary>
    /// Registers a new user account with Firebase Authentication using the provided credentials.
    /// It requests a secure session token upon successful creation to facilitate immediate authentication.
    /// </summary>
    /// <param name="email">The email address for the new user account.</param>
    /// <param name="password">The secure password for the new user account.</param>
    /// <param name="onSuccess">Callback invoked with the authentication response upon successful registration.</param>
    /// <param name="onError">Callback invoked with the error message if the registration process fails.</param>
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

    /// <summary>
    /// Generates a secure 6-digit One-Time Password (OTP), stores it locally with a 2-minute expiration, 
    /// and dispatches it to the user's email address via the EmailJS delivery service.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="onSuccess">Callback invoked if the email is successfully dispatched.</param>
    /// <param name="onError">Callback invoked if the generation or delivery process fails.</param>
    public IEnumerator SendOTP(string email,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        Debug.Log("Sending OTP to: " + email);
        // Generate a random 6-digit verification code
        string otpCode = UnityEngine.Random.Range(100000, 999999).ToString();
        // Store the OTP and its expiration timestamp locally
        PlayerPrefs.SetString("otpCode", otpCode);
        PlayerPrefs.SetString("otpExpiry",
            System.DateTime.UtcNow.AddMinutes(2).ToString());
        // Prepare the payload and send the OTP via EmailJS API
        var body = new Dictionary<string, object>{
        { "service_id", EmailJSServiceID },
        { "template_id", EmailJSTemplateID },
        { "user_id", EmailJSPublicKey },
        { "template_params", new Dictionary<string, object>
            {
                { "to_email", email },
                { "otp_code", otpCode }
            }
        } };
        yield return SendRequest(EmailJSUrl, body, onSuccess, onError);}

    /// <summary>
    /// A generic helper method to handle asynchronous HTTP POST requests using UnityWebRequest.
    /// It serializes the request body into JSON format and manages the network lifecycle, including headers and error handling.
    /// </summary>
    /// <param name="url">The target API endpoint URL.</param>
    /// <param name="body">The dictionary containing the data to be serialized into the JSON payload.</param>
    /// <param name="onSuccess">Callback invoked with the raw response text upon a successful request.</param>
    /// <param name="onError">Callback invoked with the error details if the request fails.</param>
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

    /// <summary>
    /// Validates whether a specific email address is already registered within the Firestore database.
    /// This is used to prevent duplicate account creation and verify user existence during login.
    /// </summary>
    /// <param name="email">The email address to search for in the users collection.</param>
    /// <param name="onResult">Callback invoked with a boolean indicating if the email exists.</param>
    /// <param name="onError">Callback invoked with the error message if the database query fails.</param>
    public IEnumerator CheckEmailExists(string email,
        System.Action<bool> onResult, System.Action<string> onError)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/users";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                bool exists = request.downloadHandler.text.Contains(email);
                onResult?.Invoke(exists);
            }
            else
            {
                onError?.Invoke(request.downloadHandler.text);
            }
        }
    }
    /// <summary>
    /// Retrieves user game data from Firestore using a specific email address.
    /// </summary>
    /// <param name="email">The email associated with the user account.</param>
    /// <param name="onSuccess">Callback invoked with the retrieved UserGameData upon success.</param>
    /// <param name="onError">Callback invoked with the error message upon failure.</param>
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

                string responseText = request.downloadHandler.text;
                if (responseText.Contains(email))
                {
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
    /// <summary>
    /// Verifies the email confirmation status of a user account via Firebase Authentication.
    /// It performs an account lookup using the provided ID token to check the 'emailVerified' property.
    /// </summary>
    /// <param name="idToken">The Firebase ID token for the currently authenticated user.</param>
    /// <param name="onResult">Callback invoked with a boolean value indicating if the email is verified.</param>
    /// <param name="onError">Callback invoked with the error message if the lookup request fails.</param>
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
    /// <summary>
    /// Searches for a specific User ID in Firestore by matching the provided email address.
    /// It parses the Firestore document collection to extract the unique resource identifier.
    /// </summary>
    /// <param name="email">The user's email address used to locate the document.</param>
    /// <param name="onSuccess">Callback invoked with the extracted User ID string.</param>
    /// <param name="onError">Callback invoked with the error message if the email is not found or the request fails.</param>
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