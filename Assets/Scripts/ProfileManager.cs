using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the user profile interface, allowing students to view their achievements
/// and update their personal information. Integrates with UserCoinsDisplay for currency updates.
/// </summary>
public class ProfileManager : MonoBehaviour
{
    [Header("Currency Integration")]
    public UserCoinsDisplay userCoinsDisplay; 

    [Header("UI Display")]
    public Image profileAvatar;

    [Header("Input Fields (Info)")]
    public TMP_InputField nameField;
    public TMP_InputField gradeField;
    public TMP_InputField genderField;

    [Header("Badges Container")]
    public Transform badgesContainer;
    public GameObject badgePrefab;

    [Header("Buttons")]
    public Button editSaveButton;
    public TMP_Text buttonText;

    private bool isEditing = false;
    private string currentUserId;
    private UserGameData currentUser;

    void Start()
    {
        currentUserId = PlayerPrefs.GetString("currentUserId");
        SetFieldsInteractable(false); 
        StartCoroutine(LoadProfileData());
    }

    /// <summary>
    /// Fetches full user profile from Firestore and refreshes the UI components.
    /// </summary>
    IEnumerator LoadProfileData()
    {
        PerformanceLogger.Instance.StartMeasure("Profile_Load");

        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) =>
        {
            currentUser = user;

            if (userCoinsDisplay != null) userCoinsDisplay.RefreshDisplay(); 

            nameField.text = user.name;
            gradeField.text = user.grade;
            genderField.text = user.gender;

            DisplayBadges(user.earnedBadges);
            PerformanceLogger.Instance.StopMeasure("Profile_Load");

        },
        (error) => {
            PerformanceLogger.Instance.StopMeasure("Profile_Load");
            Debug.LogError("فشل تحميل البروفايل: " + error);
        }));

    }

    void DisplayBadges(List<string> badgeIds)
    {
        foreach (Transform child in badgesContainer) Destroy(child.gameObject);

        if (badgeIds == null) return;

        foreach (string bId in badgeIds)
        {
            GameObject badgeObj = Instantiate(badgePrefab, badgesContainer);
        }
    }

    public void OnEditSaveClicked()
    {
        if (!isEditing)
        {
            isEditing = true;
            SetFieldsInteractable(true);
            buttonText.text = "حفظ";
        }
        else
        {
            SaveNewData();
        }
    }

    void SetFieldsInteractable(bool state)
    {
        nameField.interactable = state;
        gradeField.interactable = state;
        genderField.interactable = state;
    }

    /// <summary>
    /// Synchronizes updated profile information back to Firestore.
    /// </summary>
    void SaveNewData()
    {
        currentUser.name = nameField.text;
        currentUser.grade = gradeField.text;
        currentUser.gender = genderField.text;

        StartCoroutine(FirestoreManager.Instance.SaveUser(currentUser, () =>
        {
            isEditing = false;
            SetFieldsInteractable(false);
            buttonText.text = "تعديل";
            Debug.Log("تم حفظ البيانات بنجاح!");
        },
        (error) => Debug.LogError("خطأ في الحفظ: " + error)));
    }
}