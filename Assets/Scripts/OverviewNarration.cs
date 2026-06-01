using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

/// <summary>
/// يُضاف إلى أي مشهد مقدمة (Overview) لا يحتوي على StoryIntroAnimation.
/// يدير قراءة القصة صوتيًا عند فتح المشهد، ويوقف الصوت عند ضغط زر "ابدأ".
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class OverviewNarration : MonoBehaviour
{
    // ------------------- إعدادات القصة -------------------
    [Header("AI Narration Settings")]
    [Tooltip("مفتاح القصة في stories.json (mecca, south, east, qassim, riyadh, north)")]
    public string storyKey = "";          // يُحدّد في الـ Inspector لكل مرحلة
    public bool enableNarration = false; // يُفعَّل يدويًا إذا أردت الصوت

    // ------------------- مراجع UI -----------------------
    [Header("UI References")]
    [Tooltip("زر البداية داخل شاشة المقدمة (يمكن سحب الزر أو أي كائن فرعي منه)")]
    public GameObject startButton;       // يُقصد به كائن زر "ابدأ"

    // ------------------- المتغيّرات الداخلية ----------
    private string introText = "";
    private Coroutine speechCoroutine;
    private AudioSource audioSource;
    private GeminiKoreTTS koreTts;
    private static StoriesRoot cachedStories; // كاش للـ JSON لتقليل التحميل

    // ---------- WebGL TTS ----------
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WS_Speak(string text, float pitch, float rate);
    [DllImport("__Internal")]
    private static extern void WS_Stop();
#endif

    // ----------------------------------------------------------------
    void Awake()
    {
        // إعداد مكوّن AudioSource إذا لم يكن موجودًا
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        koreTts = GetComponent<GeminiKoreTTS>();
        if (koreTts == null)
            koreTts = gameObject.AddComponent<GeminiKoreTTS>();

        // ربط زر "ابدأ" لإيقاف الصوت فورًا عند الضغط
        if (startButton != null)
        {
            Button btn = startButton.GetComponent<Button>();
            if (btn == null) btn = startButton.GetComponentInChildren<Button>();
            if (btn == null) btn = startButton.GetComponentInParent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveListener(StopSpeech);
                btn.onClick.AddListener(StopSpeech);
            }
        }

        // بدء السرد إذا كان مُفعلًا
        if (enableNarration && !string.IsNullOrEmpty(storyKey))
            StartCoroutine(LoadAndSpeakSequence());
    }

    // ----------------------------------------------------------------
    private IEnumerator LoadAndSpeakSequence()
    {
        // 1️⃣ تحميل stories.json إذا لم يكن في الكاش
        if (cachedStories == null)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "stories.json");
            if (!path.StartsWith("file://")) path = "file://" + path;

            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        cachedStories = JsonConvert.DeserializeObject<StoriesRoot>(www.downloadHandler.text);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[OverviewNarration] فشل تحليل stories.json: " + e.Message);
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError("[OverviewNarration] فشل تحميل stories.json: " + www.error);
                    yield break;
                }
            }
        }

        // 2️⃣ إيجاد النص المناسب للمفتاح المحدد
        if (cachedStories?.stories != null)
        {
            foreach (var entry in cachedStories.stories)
            {
                if (string.Equals(entry.key, storyKey.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    introText = entry.intro;
                    break;
                }
            }
        }

        // 3️⃣ نطق النص إذا وجد
        if (!string.IsNullOrEmpty(introText))
            SpeakText(introText);
    }

    // ----------------------------------------------------------------
    private void SpeakText(string text)
    {
        StopSpeech(); // إيقاف أي صوت جارٍ مسبقًا

        if (koreTts != null)
        {
            koreTts.Speak(text);
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // ضبط نبرة الصوت حسب جنس اللاعب (male/female) إذا كان مُخزّنًا في PlayerPrefs
        string gender = PlayerVoiceResolver.GetStoredGender();
        float pitch = (gender == "female") ? 1.2f : 0.85f;

        WS_Stop();
        WS_Speak(text, pitch, 1.04f);
#else
        speechCoroutine = StartCoroutine(PlayEditorSpeech(text));
#endif
    }

    // ----------------------------------------------------------------
    /// <summary> إيقاف كامل لجميع الأصوات (WebGL أو Editor) </summary>
    public void StopSpeech()
    {
        if (koreTts != null)
            koreTts.Stop();

#if UNITY_WEBGL && !UNITY_EDITOR
        WS_Stop();
#endif
        if (speechCoroutine != null)
        {
            StopCoroutine(speechCoroutine);
            speechCoroutine = null;
        }
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    // ----------------------------------------------------------------
    private IEnumerator PlayEditorSpeech(string text)
    {
        // تقسيم النص إلى قطع صغرى لتفادي 400 Bad Request من Google TTS
        List<string> chunks = SplitTextIntoChunks(text, 150);

        foreach (var chunk in chunks)
        {
            string encoded = UnityWebRequest.EscapeURL(chunk);
            string url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl=ar&client=tw-ob&q={encoded}";

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null && audioSource != null)
                    {
                        audioSource.clip = clip;
                        audioSource.Play();

                        while (audioSource.isPlaying) yield return null;
                    }
                }
            }

            // فاصل بسيط بين القطع لتجنب فجوات غير مرغوبة
            yield return new WaitForSeconds(0.12f);
        }
    }

    // ----------------------------------------------------------------
    private List<string> SplitTextIntoChunks(string text, int maxChunkSize)
    {
        List<string> chunks = new List<string>();
        string[] sentences = text.Split(new char[] { '.', '!', '؟', '?', '،' }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder current = new StringBuilder();

        foreach (var s in sentences)
        {
            string trimmed = s.Trim();
            if (trimmed.Length == 0) continue;

            if (current.Length + trimmed.Length + 1 > maxChunkSize && current.Length > 0)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0) current.Append(' ');
            current.Append(trimmed);
        }

        if (current.Length > 0) chunks.Add(current.ToString());
        return chunks;
    }

    // ----------------------------------------------------------------
    private void OnDestroy() => StopSpeech();
    private void OnDisable()  => StopSpeech();

    // ------------------- تعريف بنية JSON -------------------------
    [Serializable]
    public class StoriesRoot
    {
        [JsonProperty("stories")] public List<StoryEntry> stories;
    }

    [Serializable]
    public class StoryEntry
    {
        [JsonProperty("key")]           public string key;
        [JsonProperty("intro")]         public string intro;
        // باقي الحقول موجودة في السكريبت الأصلي، لا تحتاجها هنا.
    }
}
