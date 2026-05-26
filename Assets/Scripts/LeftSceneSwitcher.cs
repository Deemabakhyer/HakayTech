using UnityEngine;

public class LeftSceneSwitcher : MonoBehaviour
{
    [Header("All left-side scenes in order")]
    [SerializeField] private GameObject[] scenes;

    [Header("Start scene index")]
    [SerializeField] private int currentSceneIndex = 0;

    private void Start()
    {
        ShowScene(currentSceneIndex);
    }

    public void ShowScene(int index)
    {
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogWarning("No scenes assigned in LeftSceneSwitcher.");
            return;
        }

        if (index < 0 || index >= scenes.Length)
        {
            Debug.LogWarning("Scene index out of range: " + index);
            return;
        }

        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i].SetActive(i == index);
        }

        currentSceneIndex = index;
    }

    public void NextScene()
    {
        int nextIndex = currentSceneIndex + 1;

        if (nextIndex < scenes.Length)
        {
            ShowScene(nextIndex);
        }
    }

    public void PreviousScene()
    {
        int previousIndex = currentSceneIndex - 1;

        if (previousIndex >= 0)
        {
            ShowScene(previousIndex);
        }
    }

    public void RestartScenes()
    {
        ShowScene(0);
    }
}