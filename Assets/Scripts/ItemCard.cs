using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCard : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text itemName;
    public TMP_Text priceText;
    public Button actionButton;
    public TMP_Text buttonText;
    public GameObject lockIcon;
    public GameObject pricePanel;
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
        // سطر جوهري: امسحي أي برمجة سابقة للزر لتجنب تداخل الأوامر
        actionButton.onClick.RemoveAllListeners();

        if (store.IsEquipped(itemData.itemId))
        {
            // حالة: مجهز ✓
            lockIcon.SetActive(false);
            pricePanel.SetActive(false);
            buttonText.text = "مجهز ✓";
            actionButton.interactable = false; // الزر لا يحتاج ضغط هنا
        }
        else if (store.IsOwned(itemData.itemId))
        {
            // حالة: مشترى - جاهز للتجهيز
            lockIcon.SetActive(false);
            pricePanel.SetActive(false);
            buttonText.text = "تجهيز";
            actionButton.interactable = true;
            actionButton.onClick.AddListener(() => store.EquipItem(itemData)); //[cite: 1]
        }
        else
        {
            // حالة: مقفل - يحتاج شراء[cite: 1]
            lockIcon.SetActive(true);
            pricePanel.SetActive(true);
            priceText.text = itemData.price.ToString();
            buttonText.text = ""; // اتركيه فارغاً لأن السعر كافٍ بصرياً
            actionButton.interactable = true;
            actionButton.onClick.AddListener(() => store.BuyItem(itemData)); //[cite: 1]
        }
    }
}