using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RiyadhIntroButtons : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button closeButton;

    [Header("Audio Source")]
    public AudioSource uiAudio;

    [Header("Button Sounds")]
    public AudioClip startClip;
    public AudioClip closeClip;

    [Header("Scenes")]
    public string riyadhGameSceneName = "Final Ver in Riyadh";
    public string homeSceneName = "HomePage";

    void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartRiyadhStory);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseIntro);
        }
    }

    public void StartRiyadhStory()
    {
        Debug.Log("RIYADH START CLICKED");

        if (uiAudio != null && startClip != null)
            uiAudio.PlayOneShot(startClip);

        SceneManager.LoadScene(riyadhGameSceneName);
    }

    public void CloseIntro()
    {
        Debug.Log("RIYADH CLOSE CLICKED");

        if (uiAudio != null && closeClip != null)
            uiAudio.PlayOneShot(closeClip);

        SceneManager.LoadScene(homeSceneName);
    }
}