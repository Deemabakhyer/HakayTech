using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    [Header("Scene Configurations")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string homeSceneName = "HomeـPage";
    [SerializeField] private string storeSceneName = "Store";     
    [SerializeField] private string profileSceneName = "Profile"; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            LoadMuteState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mapSceneName &&
            scene.name != homeSceneName &&
            scene.name != storeSceneName &&
            scene.name != profileSceneName)
        {
            instance = null; 
            Destroy(gameObject); 
        }
    }

    public void ToggleBackgroundMusic()
    {
        if (audioSource == null) return;

        audioSource.mute = !audioSource.mute;

        PlayerPrefs.SetInt("MusicMuted", audioSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadMuteState()
    {
        if (audioSource == null) return;

        bool isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        audioSource.mute = isMuted;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }
}