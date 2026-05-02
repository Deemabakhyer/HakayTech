using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Required for Coroutines

public class ButtonSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip clickClip;

    [Header("Scene Settings")]
    public string sceneToLoad;

    // This is the function you link to the Button's OnClick()
    public void OnIconClicked()
    {
        StartCoroutine(PlaySoundAndLoad());
    }

    IEnumerator PlaySoundAndLoad()
    {
        // 1. Play the sound
        if (clickClip != null)
        {
            AudioSource.PlayClipAtPoint(clickClip, Camera.main.transform.position);
        }

        // 2. Wait for a tiny fraction of a second (0.3s) 
        // This gives the audio engine time to start the clip
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