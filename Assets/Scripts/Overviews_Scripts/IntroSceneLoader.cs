using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSceneLoader : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button closeButton;

    [Header("Scene Names")]
    [Tooltip("اكتبي هنا اسم مشهد القصة")]
    public string storySceneName;

    [Tooltip("اكتبي هنا اسم مشهد الصفحة الرئيسية")]
    public string homeSceneName = "HomePage";

    void Awake()
    {
        // ربط الأزرار بالدوال البرمجية
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnStartClicked()
    {
        // يمكنكِ هنا إضافة أي لوجيك إضافي قبل الانتقال
        Invoke(nameof(LoadStoryScene), 0.2f);
    }

    private void OnCloseClicked()
    {
        Invoke(nameof(LoadHomeScene), 0.2f);
    }

    void LoadStoryScene()
    {
        if (!string.IsNullOrEmpty(storySceneName))
           SceneManager.LoadScene(storySceneName);
        else
            Debug.LogError("اسم مشهد القصة غير محدد في الـ Inspector!");
    }

    void LoadHomeScene()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}