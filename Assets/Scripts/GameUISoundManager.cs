using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUISoundManager : MonoBehaviour
{
    public static GameUISoundManager Instance;

    [Header("Audio Source")]
    public AudioSource uiAudioSource;

    [Header("UI Sounds")]
    public AudioClip popupOpenClip;
    public AudioClip buttonClickClip;
    public AudioClip closeClickClip;
    public AudioClip blockClip;
    public AudioClip typingClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float popupVolume = 0.22f;
    [Range(0f, 1f)] public float buttonVolume = 0.55f;
    [Range(0f, 1f)] public float closeVolume = 0.35f;
    [Range(0f, 2f)] public float blockVolume = 1.15f;
    [Range(0f, 1f)] public float typingVolume = 0.22f;

    [Header("AI Arrival Layered Sound")]
    public AudioSource backgroundAudio;
    public AudioClip aiArrivalClip; // الصوت الموحد
    public float aiArrivalVolume = 1f;
    public float normalBackgroundVolume = 0.10f;
    public float loweredBackgroundVolume = 0.03f;

    [Header("Scene-Controlled AI Arrival")]
    public int aiArrivalStartScene = 29;
    public int aiArrivalEndScene = 31;

    private bool aiArrivalPlayed = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        // إذا نحن ضمن نطاق النزول، شغّل الصوت الموحد
        if (currentScene >= aiArrivalStartScene && currentScene <= aiArrivalEndScene)
        {
            if (!aiArrivalPlayed)
            {
                PlayAIArrival();
            }
        }
        else
        {
            if (aiArrivalPlayed)
            {
                StopAIArrival();
            }
        }
    }

    // --------- UI Sound Methods ---------
    public void PlayPopupOpen() => Play(popupOpenClip, popupVolume);
    public void PlayButtonClick() => Play(buttonClickClip, buttonVolume);
    public void PlayCloseClick() => Play(closeClickClip, closeVolume);
    public void PlayBlock() => Play(blockClip, blockVolume);
    public void PlayTyping() => Play(typingClip, typingVolume);

    // --------- AI Arrival Methods ---------
    public void PlayAIArrival()
    {
        if (aiArrivalPlayed || aiArrivalClip == null || uiAudioSource == null)
            return;

        aiArrivalPlayed = true;

        if (backgroundAudio != null)
            backgroundAudio.volume = loweredBackgroundVolume;

        uiAudioSource.PlayOneShot(aiArrivalClip, aiArrivalVolume);
    }

    public void StopAIArrival()
    {
        if (backgroundAudio != null)
            backgroundAudio.volume = normalBackgroundVolume;

        aiArrivalPlayed = false;
    }

    // --------- Helper ---------
    private void Play(AudioClip clip, float volume)
    {
        if (uiAudioSource != null && clip != null)
            uiAudioSource.PlayOneShot(clip, volume);
    }
}