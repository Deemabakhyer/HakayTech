using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonEffects : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public AudioSource uiAudio;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    public float hoverScale = 1.08f;
    public float clickScale = 0.92f;
    public float speed = 10f;

    private Vector3 originalScale;
    private bool hovering;
    private Coroutine clickRoutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (clickRoutine != null) return;

        Vector3 target = hovering ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        if (uiAudio != null && hoverClip != null)
            uiAudio.PlayOneShot(hoverClip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiAudio != null && clickClip != null)
            uiAudio.PlayOneShot(clickClip);

        if (clickRoutine != null)
            StopCoroutine(clickRoutine);

        clickRoutine = StartCoroutine(ClickBounce());
    }

    IEnumerator ClickBounce()
    {
        transform.localScale = originalScale * clickScale;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = hovering ? originalScale * hoverScale : originalScale;
        clickRoutine = null;
    }
}