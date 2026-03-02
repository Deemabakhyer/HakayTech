using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EastrenStoryManager : MonoBehaviour
{
    [Header("Character & Animation")]
    public GameObject lulwaCharacter; // شخصية لولوة
    public SpriteRenderer characterRenderer;
    public List<Sprite> animationSprites; // صور التحريك من مصفوفة الصور

    [Header("Variable Pop-up UI")]
    public GameObject variablePopUp; // نافذة إدخال الاسم
    public TMP_InputField shellNameInput; // حقل النص

    [Header("UI Feedback")]
    public TextMeshProUGUI feedbackText; // نص المرشد التوجيهي

    // قاموس لتخزين المتغيرات (Key: الاسم، Value: نوع الصدفة)
    private Dictionary<string, string> savedVariables = new Dictionary<string, string>();

    void Start()
    {
        savedVariables.Clear();

        // إخفاء البوب أب عند بداية اللعبة
        if (variablePopUp != null)
            variablePopUp.SetActive(false);

        if (feedbackText != null)
            feedbackText.text = "انظري! لولوة وجدت صدفة جميلة.. لنعطها اسماً!";
    }

    // تفعيل زر الحفظ
    public void OnSaveVariableName()
    {
        string inputName = shellNameInput.text;

        if (!string.IsNullOrEmpty(inputName))
        {
            if (savedVariables.ContainsKey(inputName))
            {
                feedbackText.text = "هذا الاسم موجود مسبقاً، اختاري اسماً فريداً!";
            }
            else
            {
                savedVariables.Add(inputName, "Shell_Type_A");
                Debug.Log($"Variable Saved: {inputName}");

                // إغلاق النافذة وتصفير النص
                ClosePopUp();
                feedbackText.text = "اسم رائع! " + inputName + " الآن داخل العلبة.";
            }
        }
        else
        {
            feedbackText.text = "من فضلكِ اكتبي اسماً للصدفة أولاً!";
        }
    }

    // تفعيل زر إغلاق (X)
    public void ClosePopUp()
    {
        if (variablePopUp != null)
        {
            shellNameInput.text = ""; // مسح النص عند الإغلاق لضمان نظافة الحقل المرة القادمة
            variablePopUp.SetActive(false);
            Debug.Log("Pop-up closed.");
        }
    }

    // دالة لفتح البوب أب (يمكنكِ استدعاؤها عند لمس الصدفة أو سحب بلوك التسمية)
    public void OpenPopUp()
    {
        if (variablePopUp != null)
        {
            variablePopUp.SetActive(true);
            shellNameInput.ActivateInputField(); // تفعيل مؤشر الكتابة تلقائياً
        }
    }
}