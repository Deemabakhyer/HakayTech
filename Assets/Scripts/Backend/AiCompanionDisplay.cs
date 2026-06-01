using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Automatically manages the visual representation of the AI Companion (virtual mentor).
/// Fetches the user's gender from Firestore and sets the appropriate sprite (Boy/Girl).
/// </summary>
public class AiCompanionDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public Image companionImage;

    [Header("Companion Sprites")]
    public Sprite boyCompanionSprite;
    public Sprite girlCompanionSprite;

    private void OnEnable()
    {
        RefreshCompanion();
    }

    /// <summary>
    /// Synchronizes the companion's appearance with the user's profile data.
    /// </summary>
    public void RefreshCompanion()
    {
        string userId = PlayerPrefs.GetString("currentUserId");

        if (string.IsNullOrEmpty(userId)) return;

        StartCoroutine(SyncCompanionRoutine(userId));
    }

    private IEnumerator SyncCompanionRoutine(string userId)
    {
        if (FirestoreManager.Instance == null) yield break;

        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(
            userId,
            (user) =>
            {
                bool isGirl = user.gender == "أنثى" || user.gender == "female";

                if (companionImage != null)
                {
                    companionImage.sprite = isGirl ? girlCompanionSprite : boyCompanionSprite;
                }
            },
            (error) => Debug.LogError("AiCompanionDisplay: " + error)
        ));
    }
}