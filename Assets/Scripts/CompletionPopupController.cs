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
    public string storyID;
    public int winCoins = 0;

    [Header("Coin UI Animation")]
    public TextMeshProUGUI coinsAmountText;
    public RectTransform coinsIcon;
    private int initialTotalCoins = 0;

    [Header("Timing")]
    public float showDelay = 0.8f;
    public float popupDuration = 0.35f;
    public float starDelay = 0.18f;
    public float starPopDuration = 0.25f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip starClip;
    public AudioClip coinTickClip;

    private string currentUserId;

    void Awake()
    {
        currentUserId = PlayerPrefs.GetString("currentUserId");
        if (popupRoot == null) popupRoot = gameObject;
        ResetVisualsOnly();
    }

    public void ShowPopup()
    {
        popupRoot.SetActive(true);
        popupRoot.transform.SetAsLastSibling();

        StopAllCoroutines();
        ResetVisualsOnly();

        // التعديل السحري: نتحقق هل الفايربيس موجود أصلاً في المشهد وشغال؟
        if (!string.IsNullOrEmpty(currentUserId) && FindObjectOfType<FirestoreManager>() != null)
        {
            StartCoroutine(FetchAndStartSequence());
        }
        else
        {
            Debug.LogWarning("CompletionPopup: تم تشغيل الأنميشن محلياً (إما الفايربيس مفقود أو ID المستخدم غير موجود).");
            if (coinsAmountText != null) coinsAmountText.text = "+" + winCoins;
            StartCoroutine(ShowRoutine());
        }
    }

    IEnumerator FetchAndStartSequence()
    {
        bool dataLoaded = false;

        if (FirestoreManager.Instance != null)
        {
            yield return StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) => {
                initialTotalCoins = user.accumulatedCoins;
                if (coinsAmountText != null) coinsAmountText.text = initialTotalCoins.ToString();
                dataLoaded = true;
            }, (err) => {
                dataLoaded = true;
            }));

            while (!dataLoaded) yield return null;

            SaveGameProgress();
        }

        yield return StartCoroutine(ShowRoutine());
    }

    private void SaveGameProgress()
    {
        if (FirestoreManager.Instance == null) return;

        StartCoroutine(FirestoreManager.Instance.LoadUser(currentUserId, (user) =>
        {
            int newTotal = user.accumulatedCoins + winCoins;
            StartCoroutine(FirestoreManager.Instance.UpdateCoins(currentUserId, newTotal, () =>
            {
                Debug.Log("Coins updated in Firebase: " + newTotal);
            }, null));

            ProgressData progress = new ProgressData
            {
                progressId = currentUserId + "_" + storyID,
                userId = currentUserId,
                storyChallengeId = storyID,
                state = "completed",
                coins = winCoins
            };

            StartCoroutine(FirestoreManager.Instance.SaveProgress(progress, () =>
            {
                Debug.Log("Story progress saved successfully!");
            }, null));

        }, (error) => Debug.LogError("Failed to load user for update: " + error)));
    }

    IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(showDelay);
        yield return Pop(panel, 0f, 1f, popupDuration);

        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star1);
        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star2);
        yield return new WaitForSeconds(starDelay);
        yield return PopStar(star3);

        yield return new WaitForSeconds(1.2f);
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

            if (coinsAmountText.text != "+" + currentDisplay && coinTickClip != null && audioSource != null)
                audioSource.PlayOneShot(coinTickClip, 0.02f);

            coinsAmountText.text = "+" + currentDisplay;
            yield return null;
        }

        coinsAmountText.text = "+" + winCoins;
    }

    private void ResetVisualsOnly()
    {
        if (panel != null) panel.localScale = Vector3.zero;
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