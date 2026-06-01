using UnityEngine;
using UnityEngine.EventSystems;

public class HoverPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private GameObject popupCard;
    [SerializeField] private float delay = 0.2f; // Optional delay before showing

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show the card when mouse enters
        if (popupCard != null)
        {
            popupCard.SetActive(true);

            // Optional: You could add a Fade-in animation here
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the card when mouse leaves
        if (popupCard != null)
        {
            popupCard.SetActive(false);
        }
    }
}