using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ProgressBarController: Manages the visual progression state by fetching real-time 
/// completed story data directly from Firestore, aligning seamlessly with the map's unlocking logic.
/// </summary>
[RequireComponent(typeof(Image))]
public class ProgressBarController : MonoBehaviour
{
    [Header("Progress Visuals")]
    public Sprite[] progressSprites;

    [Header("Official Firebase Story IDs Order")]
    [Tooltip("اكتبي هنا الـ IDs الرسمية للقصص بنفس الترتيب وبنفس طريقة كتابتها في الفايربيس بالضبط")]
    public List<string> officialStoryIDs = new List<string> {
        "North_Story",
        "Eastern_Story", 
        "Qassim_Story",
        "Mecca_Story",
        "South_Story",
        "Riyadh_Story"
    };

    private Image progressBarImage;
    private string currentUserId;

    private void Awake()
    {
        progressBarImage = GetComponent<Image>();

        if (progressBarImage != null)
        {
            progressBarImage.enabled = false;
        }
    }

    private void OnEnable()
    {
        UpdateProgressDisplay();
    }

    public void UpdateProgressDisplay()
    {
        if (progressBarImage != null)
        {
            progressBarImage.enabled = false;
        }

        currentUserId = PlayerPrefs.GetString("currentUserId", "");

        if (!string.IsNullOrEmpty(currentUserId))
        {
            StartCoroutine(LoadProgressAndSyncUI());
        }
        else
        {
            Debug.LogWarning("ProgressBarController: No currentUserId found in PlayerPrefs.");
        }
    }

    private IEnumerator LoadProgressAndSyncUI()
    {
        bool dataLoaded = false;
        List<ProgressData> userProgress = new List<ProgressData>();

        yield return StartCoroutine(FirestoreManager.Instance.LoadAllProgress(currentUserId, (progressList) =>
        {
            userProgress = progressList;
            dataLoaded = true;
        }, (error) =>
        {
            Debug.LogError("ProgressBarController: Failed to load progress from server: " + error);
            dataLoaded = true;
        }));

        while (!dataLoaded)
        {
            yield return null;
        }

        int completedCount = 0;

        if (userProgress != null && officialStoryIDs != null)
        {
            foreach (string officialID in officialStoryIDs)
            {
                bool isDone = userProgress.Exists(p =>
                    p != null &&
                    !string.IsNullOrEmpty(p.storyChallengeId) &&
                    p.storyChallengeId.Trim() == officialID.Trim() &&
                    !string.IsNullOrEmpty(p.state) &&
                    p.state.ToLower().Trim() == "completed"
                );

                if (isDone)
                {
                    completedCount++;
                }
            }
        }

        Debug.Log($"ProgressBar: Total verified completed stories is ({completedCount}) out of ({officialStoryIDs.Count})");

        if (progressSprites == null || progressSprites.Length == 0)
        {
            Debug.LogError("ProgressBarController: progressSprites array is empty! Please assign images in the Inspector.");
            yield break;
        }

        if (completedCount < 0) completedCount = 0;
        if (completedCount >= progressSprites.Length) completedCount = progressSprites.Length - 1;

        if (progressSprites[completedCount] != null)
        {
            progressBarImage.sprite = progressSprites[completedCount];
            progressBarImage.enabled = true;
        }
    }
}