using UnityEngine;
using System.Collections;

public class AIStoryInitializer : MonoBehaviour
{
    [Header("Story Key Configuration")]
    [Tooltip("The unique key of the story from stories.json (e.g., north, east, qassim, mecca, south, riyadh)")]
    public string storyKey;

    [Header("Fallback Settings")]
    public string defaultGender = "male";

    private AICompanionController aiCompanion;

    private void Start()
    {
        // 1. Find or add AICompanionController in the scene
        aiCompanion = FindObjectOfType<AICompanionController>();
        if (aiCompanion == null)
        {
            aiCompanion = gameObject.AddComponent<AICompanionController>();
            Debug.Log("[AIStoryInitializer] AICompanionController not found. Automatically created and added to " + gameObject.name);
        }

        // 2. Fetch the player's gender and initialize
        StartCoroutine(InitializeCompanionSequence());
    }

    private IEnumerator InitializeCompanionSequence()
    {
        string gender = defaultGender;
        string userId = PlayerPrefs.GetString("currentUserId", "");

        if (!string.IsNullOrEmpty(userId) && FirestoreManager.Instance != null)
        {
            bool loaded = false;
            yield return StartCoroutine(FirestoreManager.Instance.LoadUser(
                userId,
                (user) =>
                {
                    if (user != null && !string.IsNullOrEmpty(user.gender))
                    {
                        if (user.gender == "أنثى" || user.gender == "female" || user.gender.ToLower() == "girl")
                        {
                            gender = "female";
                        }
                        else
                        {
                            gender = "male";
                        }
                    }
                    loaded = true;
                },
                (error) =>
                {
                    Debug.LogError("[AIStoryInitializer] Error loading user gender: " + error);
                    loaded = true;
                }
            ));

            // Wait a tiny bit if needed, but the callback above blocks/runs in coroutine
            while (!loaded)
            {
                yield return null;
            }
        }

        Debug.Log($"[AIStoryInitializer] Initializing companion for Story: '{storyKey}' with Gender: '{gender}'");
        PlayerVoiceResolver.SaveGender(gender);
        
        // 3. Initialize the companion
        AICompanionController.CancelAllPendingVoiceFeedback();
        aiCompanion.InitializeCompanion(storyKey, gender);

        // 4. Request the story intro (this will play the story text and trigger TTS audibly)
        aiCompanion.RequestStoryIntro();
    }
}
