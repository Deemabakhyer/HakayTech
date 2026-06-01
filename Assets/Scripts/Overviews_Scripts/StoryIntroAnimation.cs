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

public class StoryIntroAnimation : MonoBehaviour
{
    [Header("Main Panel")]
    public RectTransform storyPanel;
    public CanvasGroup storyPanelGroup;

    [Header("UI Elements to Animate")]
    public RectTransform closeButton;
    public RectTransform title;
    public RectTransform iconsRow;
    public RectTransform description;
    public RectTransform previewFrame;
    public RectTransform startButton;

    [Header("Optional Visual Effects")]
    public CanvasGroup dimOverlayGroup;
    public RectTransform panelGlow;
    public CanvasGroup panelGlowGroup;

    [Header("Audio (Intro Effect Only)")]
    public AudioSource magicWhoosh;
    public AudioClip showClip;

    [Header("AI Narration Settings")]
    [Tooltip("مفتاح القصة في stories.json (مثال: mecca, south, east, qassim, riyadh, north)")]
    public string storyKey;
    public bool enableNarration = false; // تم تعطيل النطق عند بدء المشهد؛ يمكن تفعيله يدويًا إذا رغبت

    private string introText = "";
    private Coroutine speechCoroutine;
    private AudioSource audioSource;
    private GeminiKoreTTS koreTts;
    private static StoriesRoot cachedStories;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WS_Speak(string text, float pitch, float rate);

    [DllImport("__Internal")]
    private static extern void WS_Stop();
#endif

    [Header("Timing Settings")]
    public float startDelay = 0.15f;
    public float panelDuration = 0.65f;
    public float itemDuration = 0.28f;
    public float itemDelay = 0.07f;

    void Start()
    {
        // تهيئة الصوت
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        koreTts = GetComponent<GeminiKoreTTS>();
        if (koreTts == null)
            koreTts = gameObject.AddComponent<GeminiKoreTTS>();

        // ربط زر البدء لإيقاف الصوت فوراً عند الانتقال
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

        StartCoroutine(PlayIntro());

        if (enableNarration && !string.IsNullOrEmpty(storyKey))
        {
            StartCoroutine(LoadAndSpeakSequence());
        }
    }

    IEnumerator PlayIntro()
    {
        Setup();
        Prepare();

        yield return new WaitForSeconds(startDelay);

        if (magicWhoosh != null && showClip != null)
            magicWhoosh.PlayOneShot(showClip);

        yield return FadeDim();

        yield return ShowPanel();

        yield return ShowItem(closeButton, new Vector2(-25, 25));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(title, new Vector2(0, 35));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(iconsRow, new Vector2(0, 25));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(description, new Vector2(0, 25));
        yield return new WaitForSeconds(itemDelay);
        yield return ShowItem(previewFrame, new Vector2(-35, 0));
        yield return new WaitForSeconds(itemDelay);

        yield return ShowItem(startButton, new Vector2(0, -35));

        StartCoroutine(StartButtonPulse());
        StartCoroutine(PanelGlowLoop());
    }


    void Setup()
    {
        if (storyPanelGroup == null && storyPanel != null)
        {
            storyPanelGroup = storyPanel.GetComponent<CanvasGroup>();
            if (storyPanelGroup == null)
                storyPanelGroup = storyPanel.gameObject.AddComponent<CanvasGroup>();
        }

        if (panelGlow != null && panelGlowGroup == null)
        {
            panelGlowGroup = panelGlow.GetComponent<CanvasGroup>();
            if (panelGlowGroup == null)
                panelGlowGroup = panelGlow.gameObject.AddComponent<CanvasGroup>();
        }

        if (dimOverlayGroup == null)
        {
            GameObject dim = GameObject.Find("DimOverlay");
            if (dim != null)
                dimOverlayGroup = dim.GetComponent<CanvasGroup>();
        }
    }

    void Prepare()
    {
        if (storyPanel != null) storyPanel.localScale = Vector3.one * 0.78f;
        if (storyPanelGroup != null)
        {
            storyPanelGroup.alpha = 0f;
            storyPanelGroup.interactable = false;
            storyPanelGroup.blocksRaycasts = false;
        }
        if (dimOverlayGroup != null) dimOverlayGroup.alpha = 0f;

        PrepareItem(closeButton);
        PrepareItem(title);
        PrepareItem(iconsRow);
        PrepareItem(description);
        PrepareItem(previewFrame);
        PrepareItem(startButton);

        if (panelGlowGroup != null) panelGlowGroup.alpha = 0f;
    }

    void PrepareItem(RectTransform item)
    {
        if (item == null) return;
        item.localScale = Vector3.zero;
    }

    IEnumerator FadeDim()
    {
        if (dimOverlayGroup == null) yield break;
        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            dimOverlayGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.35f);
            yield return null;
        }
        dimOverlayGroup.alpha = 1f;
    }

    IEnumerator ShowPanel()
    {
        if (storyPanel == null) yield break;
        Vector2 originalPos = storyPanel.anchoredPosition;
        float t = 0f;
        while (t < panelDuration)
        {
            t += Time.deltaTime;
            float p = t / panelDuration;
            float smooth = Mathf.SmoothStep(0, 1, p);
            storyPanel.localScale = Vector3.one * Mathf.Lerp(0.78f, 1f, smooth);
            storyPanel.anchoredPosition = Vector2.Lerp(originalPos + new Vector2(0, -45), originalPos, smooth);
            if (storyPanelGroup != null) storyPanelGroup.alpha = smooth;
            yield return null;
        }
        storyPanel.localScale = Vector3.one;
        storyPanel.anchoredPosition = originalPos;
        if (storyPanelGroup != null)
        {
            storyPanelGroup.alpha = 1f;
            storyPanelGroup.interactable = true;
            storyPanelGroup.blocksRaycasts = true;
        }
    }

    IEnumerator ShowItem(RectTransform item, Vector2 offset)
    {
        if (item == null) yield break;
        Vector2 originalPos = item.anchoredPosition;
        float t = 0f;
        while (t < itemDuration)
        {
            t += Time.deltaTime;
            float p = t / itemDuration;
            float smooth = Mathf.SmoothStep(0, 1, p);
            float bounce = Mathf.Sin(p * Mathf.PI) * 0.12f; 
            item.localScale = Vector3.one * (smooth + bounce);
            item.anchoredPosition = Vector2.Lerp(originalPos + offset, originalPos, smooth);
            yield return null;
        }
        item.localScale = Vector3.one;
        item.anchoredPosition = originalPos;
    }

    IEnumerator StartButtonPulse()
    {
        if (startButton == null) yield break;
        while (true)
        {
            yield return Scale(startButton, Vector3.one, Vector3.one * 1.05f, 0.8f);
            yield return Scale(startButton, Vector3.one * 1.05f, Vector3.one, 0.8f);
        }
    }

    IEnumerator PanelGlowLoop()
    {
        if (panelGlowGroup == null) yield break;
        while (true)
        {
            yield return FadeGlow(0f, 0.1f, 1f);
            yield return FadeGlow(0.1f, 0f, 1f);
        }
    }

    IEnumerator FadeGlow(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            panelGlowGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        panelGlowGroup.alpha = to;
    }

    IEnumerator Scale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        target.localScale = to;
    }

    private void OnDestroy()
    {
        StopSpeech();
    }

    private void OnDisable()
    {
        StopSpeech();
    }

    private IEnumerator LoadAndSpeakSequence()
    {
        // 1. تحميل ملف JSON للقصص إن لم يكن محملاً في الكاش
        if (cachedStories == null)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "stories.json");
            
            // تهيئة رابط الملف بشكل ملائم للمتصفح والـ Editor
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
                        Debug.LogError("[StoryIntroAnimation] خطأ في تحليل stories.json: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError("[StoryIntroAnimation] خطأ في تحميل stories.json: " + www.error);
                }
            }
        }

        // 2. البحث عن مقدمة القصة المطابقة للمفتاح
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

        // 3. نطق النص صوتياً
        if (!string.IsNullOrEmpty(introText))
        {
            Debug.Log("[StoryIntroAnimation] نطق مقدمة قصة: " + storyKey);
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
