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
/// هذا السكريبت يُربط ببطاقة اختيار المرحلة (Popup Card) في مشهد الخريطة (Map Scene).
/// عند تفعيل البطاقة (الضغط على مرحلة بالخريطة)، يبدأ بقراءة مقدمة القصة صوتياً.
/// وعند الضغط على زر "ابدأ" (Start) للذهاب للمرحلة، يتوقف الصوت فوراً لتفادي أي تداخل.
/// </summary>
public class MapStorySpeaker : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WS_Speak(string text, float pitch, float rate);

    [DllImport("__Internal")]
    private static extern void WS_Stop();
#endif

    [Header("Story Configuration")]
    [Tooltip("مفتاح القصة في stories.json (مثال: mecca, south, east, qassim, riyadh, north)")]
    public string storyKey;

    [Header("UI References")]
    [Tooltip("زر ابدأ الخاص بهذه البطاقة والذي ينقل اللاعب للمرحلة (يمكنك سحب كائن الزر أو أي كائن فرعي منه)")]
    public GameObject startButton;

    private string introText = "";
    private Coroutine speechCoroutine;
    private AudioSource audioSource;
    private GeminiKoreTTS koreTts;

    // كاش مشترك للقصص لعدم تحميلها من الهارد ديسك مع كل ضغطة
    private static StoriesRoot cachedStories;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        koreTts = GetComponent<GeminiKoreTTS>();
        if (koreTts == null)
            koreTts = gameObject.AddComponent<GeminiKoreTTS>();

        if (startButton != null)
        {
            // البحث التلقائي عن مكون الـ Button في الكائن المسحوب، أو في أبنائه، أو في آبائه لتجنب أي تعارض في السحب والإفلات
            Button btn = startButton.GetComponent<Button>();
            if (btn == null) btn = startButton.GetComponentInChildren<Button>();
            if (btn == null) btn = startButton.GetComponentInParent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveListener(StopSpeechAndLoad);
                btn.onClick.AddListener(StopSpeechAndLoad);
            }
            else
            {
                Debug.LogWarning("[MapStorySpeaker] تم سحب الكائن ولكن لم نجد فيه أو في فروع المكون الأساسي للـ Button.");
            }
        }
    }

    private void OnEnable()
    {
        StartCoroutine(LoadAndSpeakSequence());
    }

    private void OnDisable()
    {
        StopSpeech();
    }

    private void StopSpeechAndLoad()
    {
        StopSpeech();
    }

    private IEnumerator LoadAndSpeakSequence()
    {
        // 1. تحميل ملف JSON إذا لم يتم تحميله مسبقاً
        if (cachedStories == null)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "stories.json");
            
            // ضبط الرابط ليعمل في المتصفح أو الـ Editor
            if (!path.Contains("://") && !path.Contains(":///"))
            {
                path = "file://" + path;
            }

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
                        Debug.LogError("[MapStorySpeaker] Error parsing stories.json: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError("[MapStorySpeaker] Error loading stories.json: " + www.error);
                }
            }
        }

        // 2. البحث عن مقدمة القصة المناسبة للمفتاح
        if (cachedStories != null && cachedStories.stories != null)
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

        // 3. نطق المقدمة صوتياً
        if (!string.IsNullOrEmpty(introText))
        {
            Debug.Log("[MapStorySpeaker] نطق مقدمة قصة: " + storyKey);
            SpeakText(introText);
        }
    }

    private void SpeakText(string text)
    {
        StopSpeech();

        if (koreTts != null)
        {
            koreTts.Speak(text);
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        string gender = PlayerVoiceResolver.GetStoredGender();
        float pitch = (gender == "female") ? 1.2f : 0.85f;
        WS_Stop();
        WS_Speak(text, pitch, 1.04f);
#else
        speechCoroutine = StartCoroutine(PlayEditorSpeech(text));
#endif
    }

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
        {
            audioSource.Stop();
        }
    }

    private IEnumerator PlayEditorSpeech(string text)
    {
        // تقسيم النص لضمان عدم حدوث خطأ 400 Bad Request
        List<string> chunks = SplitTextIntoChunks(text, 150);

        foreach (string chunk in chunks)
        {
            string encodedText = UnityWebRequest.EscapeURL(chunk);
            string url = "https://translate.google.com/translate_tts?ie=UTF-8&tl=ar&client=tw-ob&q=" + encodedText;

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

                        while (audioSource.isPlaying)
                        {
                            yield return null;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.12f);
        }
    }

    private List<string> SplitTextIntoChunks(string text, int maxChunkSize)
    {
        List<string> chunks = new List<string>();
        string[] sentences = text.Split(new char[] { '.', '!', '؟', '?', '،' }, StringSplitOptions.RemoveEmptyEntries);

        StringBuilder currentChunk = new StringBuilder();
        foreach (string sentence in sentences)
        {
            string trimmed = sentence.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (currentChunk.Length + trimmed.Length + 1 > maxChunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                }
            }

            if (currentChunk.Length > 0)
                currentChunk.Append(" ");
            
            currentChunk.Append(trimmed);
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        return chunks;
    }
}
