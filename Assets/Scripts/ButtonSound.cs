using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; 

public class ButtonSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip clickClip;

    [Header("Scene Settings")]
    public string sceneToLoad;

    public void OnIconClicked()
    {
        StartCoroutine(PlaySoundAndLoad());
    }

    IEnumerator PlaySoundAndLoad()
    {
        if (clickClip != null)
        {
            AudioSource.PlayClipAtPoint(clickClip, Camera.main.transform.position);
        }
        yield return new WaitForSeconds(0.3f);

        // 3. Load the scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is empty! Sound played, but no scene to load.");
        }
    }
}