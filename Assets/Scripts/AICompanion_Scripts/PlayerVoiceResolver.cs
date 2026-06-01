using UnityEngine;

public static class PlayerVoiceResolver
{
    public const string PlayerGenderKey = "playerGender";
    public const string PendingGenderKey = "pendingGender";

    public static string NormalizeGender(string gender, string defaultGender = "male")
    {
        if (string.IsNullOrWhiteSpace(gender))
            gender = defaultGender;

        string normalized = gender.Trim().ToLower();

        if (normalized.Contains("female") ||
            normalized.Contains("girl") ||
            normalized.Contains("أنثى") ||
            normalized.Contains("انثى"))
        {
            return "female";
        }

        return "male";
    }

    public static string GetStoredGender(string defaultGender = "male")
    {
        string gender = PlayerPrefs.GetString(PlayerGenderKey, "");

        if (string.IsNullOrWhiteSpace(gender))
            gender = PlayerPrefs.GetString(PendingGenderKey, defaultGender);

        return NormalizeGender(gender, defaultGender);
    }

    public static void SaveGender(string gender, string defaultGender = "male")
    {
        PlayerPrefs.SetString(PlayerGenderKey, NormalizeGender(gender, defaultGender));
        PlayerPrefs.Save();
    }

    public static string ResolveGeminiVoice(
        string gender,
        string maleVoiceName = "Kore",
        string femaleVoiceName = "Puck")
    {
        return NormalizeGender(gender) == "female"
            ? femaleVoiceName
            : maleVoiceName;
    }
}
