using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class MapProgressManager : MonoBehaviour
{
    [System.Serializable]
    public class StoryNode
    {
        public string storyID;      // story1, story2... (ID مطابق لما في الفايربيس)
        public Button storyButton;
        public GameObject lockIcon;
    }

    [Header("Map Settings")]
    public List<StoryNode> storiesOrder; // رتبي القصص هنا بالترتيب (0 هي الأولى)

    void Start()
    {
        // نستخدم نفس المفتاح الذي استخدمناه في الهوم والمتجر
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
        bool dataLoaded = false;
        List<ProgressData> userProgress = new List<ProgressData>();

        // جلب البيانات من الفايربيس (تأكدي من وجود هذه الدالة في FirestoreManager)
        yield return StartCoroutine(FirestoreManager.Instance.LoadAllProgress(userId, (progressList) => {
            userProgress = progressList;
            dataLoaded = true;
        }, (error) => {
            Debug.LogError("Error loading progress: " + error);
            dataLoaded = true; // نفتح القفل حتى لو فشل لنطبق المنطق الافتراضي
        }));

        // انتظر حتى تكتمل عملية الجلب
        while (!dataLoaded) yield return null;

        ApplyLockLogic(userProgress);
    }

    void ApplyLockLogic(List<ProgressData> progress)
    {
        if (storiesOrder == null || storiesOrder.Count == 0) return;

        // 1. القصة الأولى (Index 0) مفتوحة دائماً للكل
        SetStoryState(storiesOrder[0], true);

        // 2. فحص بقية القصص بناءً على "اكتمال" التي قبلها
        for (int i = 1; i < storiesOrder.Count; i++)
        {
            string previousStoryID = storiesOrder[i - 1].storyID;

            // نبحث في القائمة: هل القصة السابقة موجودة وحالتها "completed"؟
            // ملاحظة: استخدمت "completed" بحروف صغيرة لتطابق ما خزنناه في بوب-اب الفوز
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
            node.lockIcon.SetActive(!isOpen); // إذا كانت مفتوحة، اخفي القفل
    }
}