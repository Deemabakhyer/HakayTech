using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlockEffects : MonoBehaviour
{
    [Header("Main Image")]
    public Image blockImage;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite wrongSprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip errorClip;

    [Header("Shake")]
    public float shakeDuration = 0.35f;
    public float shakeStrength = 10f;

    [HideInInspector] public bool isWrongMarked = false;

    private DragBlock dragBlock;
    private RectTransform rectTransform;
    private Coroutine shakeRoutine;

    void Awake()
    {
        dragBlock = GetComponent<DragBlock>();
        rectTransform = GetComponent<RectTransform>();

        if (blockImage == null)
            blockImage = GetComponent<Image>();

        if (blockImage == null)
            blockImage = GetComponentInChildren<Image>();

        if (blockImage != null && normalSprite == null)
            normalSprite = blockImage.sprite;
    }

    bool IsInsideSolutionArea()
    {
        if (transform.parent == null) return false;
        return transform.parent.name.Contains("SelectedBlocksPanel");
    }

    public void ForceCleanState()
    {
        isWrongMarked = false;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        if (blockImage != null && normalSprite != null)
            blockImage.sprite = normalSprite;

        if (blockImage != null)
            blockImage.color = Color.white;
    }

    public void ResetBlock()
    {
        if (dragBlock != null && dragBlock.isTemplate)
            return;

        if (!IsInsideSolutionArea())
            return;

        ForceCleanState();
    }

    public void ShowWrongEffect()
    {
        if (dragBlock != null && dragBlock.isTemplate)
            return;

        if (!IsInsideSolutionArea())
            return;

        isWrongMarked = true;

        if (blockImage != null && wrongSprite != null)
            blockImage.sprite = wrongSprite;

        if (audioSource != null && errorClip != null)
            audioSource.PlayOneShot(errorClip);

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        Vector2 basePos = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeStrength, shakeStrength);
            rectTransform.anchoredPosition = basePos + new Vector2(x, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = basePos;
        shakeRoutine = null;
    }
    }