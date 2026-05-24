using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RiyadhIntroButtonController : MonoBehaviour
{
    public Button startButton;
    public Button closeButton;

    public AudioSource uiAudio;
    public AudioClip startClip;
    public AudioClip closeClip;

    public string riyadhGameSceneName = "Final Ver in Riyadh";
    public string homeSceneName = "HomePage";

    void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartRiyadhStory);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseIntro);
    }

    public void StartRiyadhStory()
    {
        if (uiAudio != null && startClip != null)
            uiAudio.PlayOneShot(startClip);

        Invoke(nameof(LoadRiyadh), 0.25f);
    }

    public void CloseIntro()
    {
        if (uiAudio != null && closeClip != null)
            uiAudio.PlayOneShot(closeClip);

        Invoke(nameof(LoadHome), 0.25f);
    }

    void LoadRiyadh()
    {
        SceneManager.LoadScene(riyadhGameSceneName);
    }

    void LoadHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}