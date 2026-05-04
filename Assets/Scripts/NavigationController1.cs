using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("HomePage");
    }

    public void GoStore()
    {
        SceneManager.LoadScene("StoreButton");
    }

    public void GoProfile()
    {
        SceneManager.LoadScene("ProfileButton");
    }
}