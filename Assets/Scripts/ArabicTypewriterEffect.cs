using System.Collections;
using UnityEngine;
using TMPro;

public class ArabicTypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    
    [TextArea]
    public string fullText;

    public float characterDelay = 0.04f;

    public AudioSource audioSource;
    public AudioClip typingClip;
    public float soundInterval = 0.12f;

    private bool isTyping = false;
    private int visibleCount = 0;
    private float soundTimer = 0f;

    private string fixedArabicText = "";

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        if (textComponent == null)
        {
            Debug.LogError("Text Component is missing.");
            return;
        }

        fixedArabicText = ArabicTextFixer.Fix(fullText);

        textComponent.isRightToLeftText = false;
        textComponent.alignment = TextAlignmentOptions.TopRight;
        textComponent.text = "";

        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        visibleCount = 0;
        soundTimer = 0f;

        while (visibleCount < fixedArabicText.Length)
        {
            visibleCount++;

            // نكشف النص من النهاية حتى يظهر عربي من اليمين لليسار
            textComponent.text = fixedArabicText.Substring(fixedArabicText.Length - visibleCount);

            soundTimer += characterDelay;
            if (audioSource != null && typingClip != null && soundTimer >= soundInterval)
            {
                audioSource.PlayOneShot(typingClip);
                soundTimer = 0f;
            }

            yield return new WaitForSeconds(characterDelay);
        }

        isTyping = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isTyping)
        {
            StopAllCoroutines();
            textComponent.text = fixedArabicText;
            isTyping = false;
        }
    }
}