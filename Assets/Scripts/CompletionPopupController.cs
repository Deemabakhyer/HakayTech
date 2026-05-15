using System.Collections;
using UnityEngine;
using TMPro;

public class CompletionPopupController : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject popupRoot;
    public RectTransform panel;

    [Header("Stars")]
    public RectTransform star1;
    public RectTransform star2;
    public RectTransform star3;

    [Header("Firebase Progress")]
    public string storyID; // اكتب هنا اسم القصة (story1 مثلاً) لتطابق الداتابيس
    public int winCoins = 50;

    [Header("Coin UI Animation")]
    public TextMeshProUGUI coinsAmountText;
    public RectTransform coinsIcon;
    private int initialTotalCoins = 0; // الرصيد قبل الزيادة

    [Header("Timing")]
    public float showDelay = 0.8f;
    public float popupDuration = 0.35f;
    public float starDelay = 0.18f;
    public float starPopDuration = 0.25f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip starClip;
    public AudioClip coinTickClip;

    private string currentUserId; // نفس تعريف المتجر

    void Awake()
    {
        // جلب المعرف بنفس مفتاح المتجر لضمان الربط
        currentUserId = PlayerPrefs.GetString("currentUserId");
        if (popupRoot == null) popupRoot = gameObject;
        ResetVisualsOnly();
    }

    public void ShowPopup()
    {
        popupRoot.SetActive(true);
        popupRoot.transform.SetAsLastSibling();

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());

        // 1. نبدأ بجلب البيانات أولاً قبل الأنيميشن
        if (!string.IsNullOrEmpty(currentUserId))
        {
            StartCoroutine(FetchAndStartSequence());
        }

        // تنفيذ الحفظ للسيرفر
        if (!string.IsNullOrEmpty(currentUserId))
        {
            SaveGameProgress();
        }
        else
        {
            Debug.LogError("CompletionPopup: No currentUserId found in PlayerPrefs!");
        }
    }

    IEnumerator FetchAndStartSequence()
    {
        // جلب الرصيد الحالي من السيرفر قبل البدء
        bool dataLoaded = false;
        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) => {
            initialTotalCoins = user.accumulatedCoins;
            // عرض الرصيد الحالي فوراً (بدل الصفر)
            coinsAmountText.text = initialTotalCoins.ToString();
            dataLoaded = true;
        }, (err) => {
            dataLoaded = true; // نفتح القفل حتى لو فشل لجعل اللعبة تستمر
        }));

        while (!dataLoaded) yield return null;

        // 2. البدء في أنيميشن اللوحة والنجوم
        StartCoroutine(ShowRoutine());

        // 3. حفظ البيانات في السيرفر في الخلفية
        SaveGameProgress();
    }

    // --- منطق الفايربيس (مطابق للوجك المتجر) ---
    private void SaveGameProgress()
    {
        // 1. جلب بيانات المستخدم الحالية للتأكد من رصيد الكوينز
        StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) =>
        {
            // 2. تحديث الكوينز
            int newTotal = user.accumulatedCoins + winCoins;
            StartCoroutine(FirestoreManager.Instance.UpdateCoins(currentUserId, newTotal, () =>
            {
                Debug.Log("Coins updated in Firebase: " + newTotal);
            }, null));

            // 3. حفظ التقدم (Progress)
            ProgressData progress = new ProgressData
            {
                progressId = currentUserId + "_" + storyID,
                userId = currentUserId,
                storyChallengeId = storyID,
                state = "completed", // تأكدي أنها حروف صغيرة
                coins = winCoins
            };

            StartCoroutine(FirestoreManager.Instance.SaveProgress(progress, () =>
            {
                Debug.Log("Story progress saved successfully!");
            }, null));

        }, (error) => Debug.LogError("Failed to load user for update: " + error)));
    }

    // --- الأنيميشن والعرض البصري ---
    IEnumerator ShowRoutine()
    {
        ResetVisualsOnly();
        yield return new WaitForSeconds(showDelay);
        yield return Pop(panel, 0f, 1f, popupDuration);

        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star1);
        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star2);
        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star3);

        yield return new WaitForSeconds(0.2f);
        yield return AnimateCoinCounter();
    }

    IEnumerator AnimateCoinCounter()
    {
        if (coinsAmountText == null) yield break;

        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int currentDisplay = (int)Mathf.Lerp(0, winCoins, elapsed / duration);

            if (coinsAmountText.text != "+" + currentDisplay && coinTickClip != null)
                audioSource.PlayOneShot(coinTickClip, 0.4f);
            coinsAmountText.text = "+" + currentDisplay;
            yield return null;
        }
    }

    // --- الدوال المساعدة ---
    private void ResetVisualsOnly()
    {
        if (panel != null) panel.localScale = Vector3.zero;
        // هنا قمنا بإزالة السطر الذي يصفر النص (coinsAmountText.text = "+0")
        // لكي يظل الرقم الذي جلبناه من السيرفر (initialTotalCoins) ظاهراً
        if (star1 != null) star1.localScale = Vector3.zero;
        if (star2 != null) star2.localScale = Vector3.zero;
        if (star3 != null) star3.localScale = Vector3.zero;
    }

    IEnumerator PopStar(RectTransform star)
    {
        if (audioSource != null && starClip != null)
            audioSource.PlayOneShot(starClip);
        yield return Pop(star, 0f, 1f, starPopDuration);
    }

    IEnumerator Pop(RectTransform target, float from, float to, float duration)
    {
        if (target == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, p);
            target.localScale = Vector3.one * Mathf.Lerp(from, to, smooth);
            yield return null;
        }
        target.localScale = Vector3.one * to;
    }
}
