using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    private void Awake()
    {
        // Singleton pattern: ensures only one music player exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}