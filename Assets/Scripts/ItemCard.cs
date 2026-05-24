using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ItemCard: Controls the visual state and interactive behavior of store item cards, 
/// toggling price configurations, lock overlays, and custom checkmark assets based on item ownership.
/// </summary>
public class ItemCard : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text itemName;
    public TMP_Text priceText;
    public Button actionButton;
    public TMP_Text buttonText;
    public GameObject lockIcon;
    public GameObject pricePanel;

    [Header("Custom Icons")]
    public GameObject checkmarkIcon;

    private ItemData itemData;
    private StoreManager store;

    public void Setup(ItemData item, StoreManager storeManager)
    {
        itemData = item;
        store = storeManager;
        itemImage.sprite = item.itemImage;
        itemName.text = item.itemName;

        UpdateCardState();
    }

    void UpdateCardState()
    {
        actionButton.onClick.RemoveAllListeners();

        if (store.IsEquipped(itemData.itemId))
        {
            lockIcon.SetActive(false);
            pricePanel.SetActive(false);

            if (checkmarkIcon != null) checkmarkIcon.SetActive(true);

            buttonText.text = "مجهز";
            actionButton.interactable = false;
        }
        else if (store.IsOwned(itemData.itemId))
        {
            lockIcon.SetActive(false);
            pricePanel.SetActive(false);

            if (checkmarkIcon != null) checkmarkIcon.SetActive(false);

            buttonText.text = "تجهيز";
            actionButton.interactable = true;
            actionButton.onClick.AddListener(() => store.EquipItem(itemData));
        }
        else
        {
            lockIcon.SetActive(true);
            pricePanel.SetActive(true);

            if (checkmarkIcon != null) checkmarkIcon.SetActive(false);

            priceText.text = itemData.price.ToString();
            buttonText.text = "";
            actionButton.interactable = true;
            actionButton.onClick.AddListener(() => store.BuyItem(itemData));
        }
    }
}