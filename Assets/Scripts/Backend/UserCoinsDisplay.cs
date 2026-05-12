using UnityEngine;
using TMPro;
using System.Collections;
/// <summary>
/// A reusable component to fetch and display the current user's coin balance from Firestore.
/// Can be attached to any UI element across different scenes.
/// </summary>
public class UserCoinsDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text coinsText;

    private void OnEnable()
    {
        RefreshDisplay();
    }
    /// <summary>
    /// Public method to manually trigger a coin balance refresh.
    /// </summary>
    public void RefreshDisplay()
    {
        string userId = PlayerPrefs.GetString("currentUserId");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("UserCoinsDisplay: No currentUserId found in PlayerPrefs.");
            return;
        }

        StartCoroutine(FetchCoinsRoutine(userId));
    }
    private IEnumerator FetchCoinsRoutine(string userId)
    {
        if (FirestoreManager.Instance == null)
        {
            Debug.LogError("UserCoinsDisplay: FirestoreManager instance is missing!");
            yield break;
        }

        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(
            userId,
            (user) =>
            {
                if (coinsText != null)
                    coinsText.text = user.accumulatedCoins.ToString();
            },
            (error) => Debug.LogError("UserCoinsDisplay Error: " + error)
        ));
    }
}