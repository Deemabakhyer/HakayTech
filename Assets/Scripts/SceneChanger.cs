using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene(string sceneName) { 
        SceneManager.LoadScene(sceneName);

    }
}