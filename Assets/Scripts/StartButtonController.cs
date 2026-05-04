using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonController : MonoBehaviour
{
    [Header("Button")]
    public Button startButton;

    [Header("Scene")]
    public string nextSceneName = "";

    [Header("Characters")]
    public RectTransform assistant;
    public RectTransform bubbleEmpty;
    public RectTransform bubbleWithText;

    [Header("Audio")]
    public AudioSource uiAudio;
    public AudioClip startClip;

    private bool isStarting = false;

    void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
            startButton.onClick.AddListener(StartGame);
        }
    }

    public void StartGame()
    {
        Debug.Log("START CLICKED");

        if (isStarting) return;
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        isStarting = true;

        if (uiAudio != null && startClip != null)
            uiAudio.PlayOneShot(startClip);

        if (bubbleEmpty != null)
            bubbleEmpty.gameObject.SetActive(false);

        if (bubbleWithText != null)
            bubbleWithText.gameObject.SetActive(false);

        if (assistant != null)
        {
            Vector3 originalScale = assistant.localScale;

            assistant.localScale = originalScale * 1.08f;
            yield return new WaitForSeconds(0.08f);

            assistant.localScale = originalScale * 0.96f;
            yield return new WaitForSeconds(0.08f);

            assistant.localScale = originalScale;
        }

        yield return new WaitForSeconds(0.15f);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}