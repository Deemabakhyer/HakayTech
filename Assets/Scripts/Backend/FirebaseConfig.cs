public static class FirebaseConfig
{
    //Initializes app’s connection to Firebase services
    public const string ApiKey = "AIzaSyA-hIcFFgqnDobRchckKuPY4UZH5ivJYQk";
    public const string AuthDomain = "hakaytech-c4822.firebaseapp.com";
    public const string ProjectId = "hakaytech-c4822";
    public const string SignUpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + ApiKey;
    public const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + ApiKey;
    public const string SendOtpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + ApiKey;
}
