using UnityEngine;
using UnityEngine.UI;

public class MusicToggle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    private bool isMuted = false;

    void Start()
    {
        // Optional: Load previous state from PlayerPrefs
        isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        UpdateMusicState();
    }

    public void ToggleMusic()
    {
        isMuted = !isMuted;

        // Save state so it persists between scenes/restarts
        PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);

        UpdateMusicState();
    }

    private void UpdateMusicState()
    {
        // 1. Update the UI Icon
        buttonImage.sprite = isMuted ? musicOffSprite : musicOnSprite;

        // 2. Mute/Unmute Audio
        // Setting master volume is the cleanest way to mute "all music"
        GameObject.Find("MusicManager").GetComponent<AudioSource>().mute = isMuted;
        // OR: If you only want to mute music but keep UI SFX, use:
        // GameObject.FindAnyObjectByType<MusicManager>().GetComponent<AudioSource>().mute = isMuted;
    }
}