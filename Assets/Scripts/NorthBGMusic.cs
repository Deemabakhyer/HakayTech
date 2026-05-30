using UnityEngine;
using UnityEngine.SceneManagement;

public class NorthBGMusic : MonoBehaviour
{
    private static NorthBGMusic instance;
    private AudioSource audioSource;

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
}