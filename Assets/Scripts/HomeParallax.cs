using UnityEngine;

public class HomeParallax : MonoBehaviour
{
    public RectTransform background;
    public RectTransform starsLayer;

    public float backgroundAmount = 12f;
    public float starsAmount = 28f;
    public float smoothSpeed = 5f;

    private Vector2 bgStart;
    private Vector2 starsStart;

    void Start()
    {
        if (background != null)
            bgStart = background.anchoredPosition;

        if (starsLayer != null)
            starsStart = starsLayer.anchoredPosition;
    }

    void Update()
    {
        Vector2 mouse = Input.mousePosition;

        float x = (mouse.x / Screen.width - 0.5f) * 2f;
        float y = (mouse.y / Screen.height - 0.5f) * 2f;

        if (background != null)
        {
            Vector2 target = bgStart + new Vector2(x, y) * backgroundAmount;
            background.anchoredPosition =
                Vector2.Lerp(background.anchoredPosition, target, Time.deltaTime * smoothSpeed);
        }

        if (starsLayer != null)
        {
            Vector2 target = starsStart + new Vector2(x, y) * starsAmount;
            starsLayer.anchoredPosition =
                Vector2.Lerp(starsLayer.anchoredPosition, target, Time.deltaTime * smoothSpeed);
        }
    }
}
