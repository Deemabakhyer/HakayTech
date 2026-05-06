using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.UIElements.UxmlAttributeDescription;

/// <summary>
/// Manages the virtual store, item purchasing, and equipment logic.
/// Delegating coin display updates to the UserCoinsDisplay component.
/// </summary>
public class StoreManager : MonoBehaviour
{
    [Header("Character Display")]
    public Image characterDisplay;
    public Sprite boySprite;
    public Sprite girlSprite;

    [Header("Items")]
    public ItemData[] boyItems;
    public ItemData[] girlItems;
    public Transform itemsContainer;
    public GameObject itemCardPrefab;

    [Header("Currency Integration")]
    public UserCoinsDisplay userCoinsDisplay; // اسحبي سكربت عرض الكوينز هنا
    [Header("Companion Integration")]
    public AiCompanionDisplay aiDisplay;

    private string currentUserId;
    private string currentGender;
    private int currentCoins;
    private List<string> ownedItemIds = new List<string>();
    private string equippedItemId = "";

    void Start()
    {
        currentUserId = PlayerPrefs.GetString("currentUserId");
        StartCoroutine(LoadUserData());
    }


    /// <summary>
    /// Fetches all necessary user data from Firestore and synchronizes independent UI components.
    /// This includes coins, the AI companion appearance, and purchased store items.
    /// </summary>
    IEnumerator LoadUserData()
    {
        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(
            currentUserId,
            (user) =>
            {
            currentGender = user.gender; 
                currentCoins = user.accumulatedCoins;

                if (userCoinsDisplay != null)
            userCoinsDisplay.RefreshDisplay(); 

                if (aiDisplay != null)
            aiDisplay.RefreshCompanion(); 

                equippedItemId = PlayerPrefs.GetString("LastEquipped_" + currentUserId, ""); 

                StartCoroutine(FirestoreManager.Instance.LoadOwnedItems(
                    currentUserId,
                    (items) => {
                    ownedItemIds.Clear();
                    foreach (var item in items)
                    {
                        ownedItemIds.Add(item.itemId);
                        if (item.equipped) equippedItemId = item.itemId; 
                        }

                        if (!string.IsNullOrEmpty(equippedItemId))
                            ApplyEquippedSprite(equippedItemId); 

    LoadItems(); 
},
                    (error) => Debug.LogError("Error fetching owned items: " + error)
                ));
            },
            (error) => Debug.LogError("Failed to load user: " + error)
        ));
    }


    public void BuyItem(ItemData item)
    {
        if (currentCoins < item.price)
        {
            Debug.LogWarning("Not enough coins!");
            return;
        }

        // 1. الخصم محلياً
        currentCoins -= item.price;
        ownedItemIds.Add(item.itemId);

        // 2. تحديث الداتابيس
        StartCoroutine(FirestoreManager.Instance.UpdateCoins(
            currentUserId, currentCoins,
            () => {
                // 3. تحديث الواجهة فورياً عبر السكربت المشترك بعد التأكد من نجاح الخصم
                if (userCoinsDisplay != null) userCoinsDisplay.RefreshDisplay();
            },
            (error) => Debug.LogError(error)
        ));

        OwnedItem newItem = new OwnedItem
        {
            ownedItemId = System.Guid.NewGuid().ToString(),
            userId = currentUserId,
            itemId = item.itemId,
            equipped = false
        };

        StartCoroutine(FirestoreManager.Instance.SaveOwnedItem(
            newItem,
            () => LoadItems(),
            (error) => Debug.LogError(error)
        ));
    }
    void LoadItems()
    {
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        bool isGirl = currentGender == "أنثى" || currentGender == "female";
        ItemData[] items = isGirl ? girlItems : boyItems;

        foreach (ItemData item in items)
        {
            GameObject card = Instantiate(itemCardPrefab, itemsContainer);
            ItemCard itemCard = card.GetComponent<ItemCard>();
            itemCard.Setup(item, this);
        }
    }

    public bool IsOwned(string itemId) => ownedItemIds.Contains(itemId);
    public bool IsEquipped(string itemId) => equippedItemId == itemId;

    public void EquipItem(ItemData item)
    {
        equippedItemId = item.itemId;
        characterDisplay.sprite = item.characterSprite;
        PlayerPrefs.SetString("LastEquipped_" + currentUserId, item.itemId);
        LoadItems();
    }

    void ApplyEquippedSprite(string itemId)
    {
        bool isGirl = currentGender == "أنثى" || currentGender == "female";
        ItemData[] items = isGirl ? girlItems : boyItems;

        foreach (var item in items)
        {
            if (item.itemId == itemId)
            {
                characterDisplay.sprite = item.characterSprite;
                break;
            }
        }
    }
}