using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupHandler : MonoBehaviour
{
    public GameObject pausePopup;
    public GameObject confirmPopup;

    public AudioSource uiAudio;     // المشغل
    public AudioClip popupSound;    // صوت البوب اب

    public void ShowPausePopup()
    {
        pausePopup.SetActive(true);
        Time.timeScale = 0f;

        // تشغيل الصوت
        if (uiAudio != null && popupSound != null)
            uiAudio.PlayOneShot(popupSound);
    }

    public void HidePausePopup()
    {
        pausePopup.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowConfirmPopup()
    {
        pausePopup.SetActive(false);
        confirmPopup.SetActive(true);

        // تشغيل الصوت
        if (uiAudio != null && popupSound != null)
            uiAudio.PlayOneShot(popupSound);
    }

    public void BackToPause()
    {
        confirmPopup.SetActive(false);
        pausePopup.SetActive(true);

        // تشغيل الصوت (اختياري)
        if (uiAudio != null && popupSound != null)
            uiAudio.PlayOneShot(popupSound);
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}