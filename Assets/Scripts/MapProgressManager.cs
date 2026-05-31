using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class MapProgressManager : MonoBehaviour
{
    [System.Serializable]
    public class StoryNode
    {
        public string storyID;      
        public Button storyButton;
        public GameObject lockIcon;
    }

    [Header("Map Settings")]
    public List<StoryNode> storiesOrder; 

    void Start()
    {
        string userId = PlayerPrefs.GetString("currentUserId", "");

        if (!string.IsNullOrEmpty(userId))
        {
            StartCoroutine(UpdateMapProgress(userId));
        }
        else
        {
            Debug.LogError("MapManager: No currentUserId found!");
        }
    }

    IEnumerator UpdateMapProgress(string userId)
    {
        PerformanceLogger.Instance.StartMeasure("Map_Load");

        bool dataLoaded = false;
        List<ProgressData> userProgress = new List<ProgressData>();

        yield return StartCoroutine(FirestoreManager.Instance.LoadAllProgress(userId, (progressList) => {
            userProgress = progressList;
            dataLoaded = true;
        }, (error) => {
            Debug.LogError("Error loading progress: " + error);
            dataLoaded = true; 
        }));

        while (!dataLoaded) yield return null;
        PerformanceLogger.Instance.StopMeasure("Map_Load");
        ApplyLockLogic(userProgress);
    }

    void ApplyLockLogic(List<ProgressData> progress)
    {
        if (storiesOrder == null || storiesOrder.Count == 0) return;

        SetStoryState(storiesOrder[0], true);

        for (int i = 1; i < storiesOrder.Count; i++)
        {
            string previousStoryID = storiesOrder[i - 1].storyID;

            bool isPreviousDone = progress.Exists(p =>
                p.storyChallengeId == previousStoryID &&
                p.state.ToLower() == "completed"
            );

            SetStoryState(storiesOrder[i], isPreviousDone);
        }
    }

    void SetStoryState(StoryNode node, bool isOpen)
    {
        if (node.storyButton != null)
            node.storyButton.interactable = isOpen;

        if (node.lockIcon != null)
            node.lockIcon.SetActive(!isOpen); 
    }
}