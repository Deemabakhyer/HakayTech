using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    [SerializeField] private string mapSceneName = "Map"; // Set this to your scene name

    private void Awake()
    {
        // 1. Standard Singleton Logic
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            // 2. Ensure music actually starts if not playing
            if (!audioSource.isPlaying) audioSource.Play();
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
        // 3. If we are NOT in the Map Scene anymore, kill this object
        if (scene.name != mapSceneName)
        {
            instance = null; // Clear the reference
            Destroy(gameObject); // This stops the music permanently
        }
    }
}