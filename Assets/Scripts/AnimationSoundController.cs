using UnityEngine;

public class AnimationSoundController : MonoBehaviour
{
    public AudioSource audioSource;

    void Awake()
    {
        // If you forgot to drag it in the inspector, 
        // this line will go find it for you.
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        // 1. Check if we have a speaker (AudioSource)
        if (audioSource == null)
        {
            // Try one last time to find it automatically
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogError($"CRITICAL: {gameObject.name} is trying to play sound but has no AudioSource component!");
                return; // Exit the function so we don't crash
            }
        }

        // 2. Check if the Animation Event actually passed a sound clip
        if (clip == null)
        {
            Debug.LogWarning($"MISSING CLIP: An animation event on {gameObject.name} fired, but no AudioClip was dragged into the Event box.");
            return; // Exit
        }

        // 3. If both are present, play the sound
        audioSource.PlayOneShot(clip);
    }
}