using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProfileManager : MonoBehaviour
{
    [Header("UI Display")]
    public TMP_Text coinsText;
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
        currentUserId = PlayerPrefs.GetString("currentUserId"); //
        SetFieldsInteractable(false); // البداية عرض فقط
        StartCoroutine(LoadProfileData());
    }

    IEnumerator LoadProfileData()
    {
        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) =>
        {
            currentUser = user;
            // عرض الكوينز والمعلومات الأساسية
            coinsText.text = user.accumulatedCoins.ToString();
            nameField.text = user.name;
            gradeField.text = user.grade;
            genderField.text = user.gender;

            // عرض الشارات (Badges)
            DisplayBadges(user.earnedBadges);
        },
        (error) => Debug.LogError("فشل تحميل البروفايل: " + error)));
    }

    void DisplayBadges(List<string> badgeIds)
    {
        // مسح الشارات القديمة
        foreach (Transform child in badgesContainer) Destroy(child.gameObject);

        if (badgeIds == null) return;

        foreach (string bId in badgeIds)
        {
            GameObject badgeObj = Instantiate(badgePrefab, badgesContainer);
            // هنا يمكنكِ ربط السبرايت الخاص بالشارة بناءً على الـ ID
        }
    }

    public void OnEditSaveClicked()
    {
        if (!isEditing)
        {
            // دخول وضع التعديل
            isEditing = true;
            SetFieldsInteractable(true);
            buttonText.text = "حفظ";
        }
        else
        {
            // حفظ البيانات في Firestore
            SaveNewData();
        }
    }

    void SetFieldsInteractable(bool state)
    {
        nameField.interactable = state;
        gradeField.interactable = state;
        genderField.interactable = state;
    }

    void SaveNewData()
    {
        currentUser.name = nameField.text;
        currentUser.grade = gradeField.text;
        currentUser.gender = genderField.text;

        // تحديث المستخدم بالكامل في Firestore
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
