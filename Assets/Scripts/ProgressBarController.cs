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
        if (userProgress != null)
        {
            foreach (var progress in userProgress)
            {
                if (progress != null && !string.IsNullOrEmpty(progress.state) && progress.state.ToLower().Trim() == "completed")
                {
                    completedCount++;
                }
            }
        }

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
