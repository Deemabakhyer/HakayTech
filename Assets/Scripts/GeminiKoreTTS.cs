using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

[RequireComponent(typeof(AudioSource))]
public class GeminiKoreTTS : MonoBehaviour
{
    [Header("Gemini TTS")]
    [SerializeField] private string geminiApiKey = "AIzaSyALPqg4fyA7n1PyIFl-OWl75aMRJ71954M";
    [SerializeField] private string maleVoiceName = "Kore";
    [SerializeField] private string femaleVoiceName = "Puck";
    [SerializeField] private int maxChunkSize = 220;

    private const string TTS_MODEL_ID = "gemini-2.5-flash-preview-tts";
    private const int SAMPLE_RATE = 24000;

    private AudioSource audioSource;
    private Coroutine speechCoroutine;
    private int speechGeneration;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void WS_Speak(string text, float pitch, float rate);
    [DllImport("__Internal")] private static extern void WS_Stop();
#endif

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void Speak(string text)
    {
        Stop();

        if (string.IsNullOrWhiteSpace(text))
            return;

        int currentGeneration = ++speechGeneration;
        speechCoroutine = StartCoroutine(SpeakRoutine(text, currentGeneration));
    }

    public void Stop()
    {
        speechGeneration++;

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

    private IEnumerator SpeakRoutine(string text, int currentGeneration)
    {
        List<string> chunks = SplitTextIntoChunks(text, maxChunkSize);

        foreach (string chunk in chunks)
        {
            if (currentGeneration != speechGeneration)
                yield break;

            bool playedWithGemini = false;

            yield return StartCoroutine(TryPlayGeminiChunk(chunk, currentGeneration, success =>
            {
                playedWithGemini = success;
            }));

            if (currentGeneration != speechGeneration)
                yield break;

            if (!playedWithGemini)
                yield return StartCoroutine(PlayFallbackChunk(chunk, currentGeneration));

            yield return new WaitForSeconds(0.08f);
        }
    }

    private IEnumerator TryPlayGeminiChunk(string text, int currentGeneration, Action<bool> onDone)
    {
        string prompt =
            "[warm, clear, natural Arabic narration, child-friendly, slightly brisk pace, about 1.12x speed, keep every word clear, do not rush] " +
            "Say exactly this Arabic text: \"" + text + "\"";

        string jsonBody = JsonConvert.SerializeObject(new GeminiRequest
        {
            contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    role = "user",
                    parts = new List<GeminiPart>
                    {
                        new GeminiPart { text = prompt }
                    }
                }
            },
            generationConfig = new GeminiGenerationConfig
            {
                responseModalities = new List<string> { "AUDIO" },
                speechConfig = new GeminiSpeechConfig
                {
                    voiceConfig = new GeminiVoiceConfig
                    {
                        prebuiltVoiceConfig = new GeminiPrebuiltVoiceConfig
                        {
                            voiceName = ResolveVoiceName()
                        }
                    }
                }
            }
        });

        string fullUrl = "https://generativelanguage.googleapis.com/v1beta/models/" +
                         TTS_MODEL_ID + ":generateContent?key=" + geminiApiKey;

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (currentGeneration != speechGeneration)
                yield break;

            if (request.result == UnityWebRequest.Result.Success &&
                TryCreateAudioClip(request.downloadHandler.text, out AudioClip clip))
            {
                if (currentGeneration != speechGeneration)
                    yield break;

                audioSource.clip = clip;
                audioSource.Play();

                while (audioSource.isPlaying)
                {
                    if (currentGeneration != speechGeneration)
                    {
                        audioSource.Stop();
                        yield break;
                    }

                    yield return null;
                }

                onDone?.Invoke(true);
                yield break;
            }

            Debug.LogWarning("[GeminiKoreTTS] Kore failed, using fallback. " + request.error);
        }

        onDone?.Invoke(false);
    }

    private bool TryCreateAudioClip(string jsonResponse, out AudioClip clip)
    {
        clip = null;

        try
        {
            GeminiResponse response = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);
            GeminiPart part = response?.candidates?[0]?.content?.parts?[0];
            string base64Audio = part?.inlineData?.data ?? part?.inline_data?.data;

            if (string.IsNullOrEmpty(base64Audio))
                return false;

            byte[] pcmBytes = Convert.FromBase64String(base64Audio);
            int sampleCount = pcmBytes.Length / 2;
            if (sampleCount <= 0)
                return false;

            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(pcmBytes, i * 2);
                samples[i] = Mathf.Clamp(sample / 32768f, -1f, 1f);
            }

            clip = AudioClip.Create("GeminiKoreNarration", sampleCount, 1, SAMPLE_RATE, false);
            return clip.SetData(samples, 0);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GeminiKoreTTS] Could not parse audio: " + e.Message);
            return false;
        }
    }

    private string ResolveVoiceName()
    {
        return PlayerVoiceResolver.ResolveGeminiVoice(
            PlayerVoiceResolver.GetStoredGender(),
            maleVoiceName,
            femaleVoiceName
        );
    }

    private IEnumerator PlayFallbackChunk(string text, int currentGeneration)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (currentGeneration != speechGeneration)
            yield break;

        string gender = PlayerVoiceResolver.GetStoredGender();
        float pitch = gender == "female" ? 1.2f : 0.85f;
        WS_Stop();
        WS_Speak(text, pitch, 1.04f);
        yield break;
#else
        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = "https://translate.google.com/translate_tts?ie=UTF-8&tl=ar&client=tw-ob&q=" + encodedText;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (currentGeneration != speechGeneration)
                yield break;

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    if (currentGeneration != speechGeneration)
                        yield break;

                    audioSource.clip = clip;
                    audioSource.Play();

                    while (audioSource.isPlaying)
                    {
                        if (currentGeneration != speechGeneration)
                        {
                            audioSource.Stop();
                            yield break;
                        }

                        yield return null;
                    }
                }
            }
        }
#endif
    }

    private List<string> SplitTextIntoChunks(string text, int maxSize)
    {
        List<string> chunks = new List<string>();
        string[] sentences = text.Split(new char[] { '.', '!', '؟', '?', '،' }, StringSplitOptions.RemoveEmptyEntries);

        StringBuilder current = new StringBuilder();
        foreach (string sentence in sentences)
        {
            string trimmed = sentence.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (current.Length + trimmed.Length + 1 > maxSize && current.Length > 0)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0)
                current.Append(' ');

            current.Append(trimmed);
        }

        if (current.Length > 0)
            chunks.Add(current.ToString());

        return chunks;
    }
}
