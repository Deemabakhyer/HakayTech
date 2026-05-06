using UnityEngine;
using UnityEngine.UI;

public class BlockGlowPulse : MonoBehaviour
{
    public Image glowImage;

    [Header("Pulse")]
    public float minAlpha = 0.25f;
    public float maxAlpha = 0.75f;
    public float pulseSpeed = 2.2f;

    [Header("Scale Pulse")]
    public float minScale = 1.0f;
    public float maxScale = 1.06f;

    private Color baseColor;
    private RectTransform rectTransform;

    void Awake()
    {
        if (glowImage == null)
            glowImage = GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();

        if (glowImage != null)
            baseColor = glowImage.color;
    }

    void Update()
    {
        if (glowImage == null || rectTransform == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        glowImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        float scale = Mathf.Lerp(minScale, maxScale, t);
        rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}