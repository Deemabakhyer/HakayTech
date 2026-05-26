using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Popup")]
    public GameObject pausePopup;

    private bool isPaused = false;

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePopup != null)
            pausePopup.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePopup != null)
            pausePopup.SetActive(false);

        Time.timeScale = 1f;
    }
}
