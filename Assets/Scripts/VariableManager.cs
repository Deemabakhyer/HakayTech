using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VariableManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;      // اسحبي لوحة البوب أب هنا
    public TMP_InputField inputField;  // اسحبي حقل النص هنا
    public TextMeshProUGUI varNameText; // النص الذي سيظهر على الدائرة لاحقاً
    public GameObject variableCircle;  // الدائرة التي سيظهر عليها اسم المتغير
    [Header("Audio")]
    public AudioClip beachSound; // غيرناها هنا إلى AudioClip
    private AudioSource myAudioSource;



    void Start()
    {
        // إخفاء البوب أب والدائرة عند بداية اللعبة
        popupPanel.SetActive(false);
        variableCircle.SetActive(false);

        // ننشأ أو نأخذ المكون برمجياً ليعمل كالمسجل
        myAudioSource = GetComponent<AudioSource>();
        if (myAudioSource == null)
        {
            myAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (beachSound != null)
        {
            myAudioSource.clip = beachSound;
            myAudioSource.loop = true;
            myAudioSource.Play();
        }
    }

    // تُستدعى عند الضغط على زر "إنشاء متغير"
    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        inputField.text = ""; // تنظيف الحقل
    }

    // تُستدعى عند الضغط على زر X
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    // تُستدعى عند الضغط على زر "حفظ"
    public void SaveVariable()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            string name = inputField.text;
            varNameText.text = name; // عرض الاسم على الدائرة

            variableCircle.SetActive(true); // إظهار الدائرة في قائمة البلوكات
            ClosePopup(); // إغلاق النافذة

            Debug.Log("تم إنشاء متغير باسم: " + name);
        }
    }
}