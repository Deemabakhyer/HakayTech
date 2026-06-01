using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupButtons : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("Map");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Next()
    {
        SceneManager.LoadScene("Map");
    }
}
