using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// StoreManager: Manages virtual store transactions and equipment flows, ensuring strict 
/// gender-to-item sequence validation before triggering any visual rendering updates.
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
    public UserCoinsDisplay userCoinsDisplay;
    [Header("Companion Integration")]
    public AiCompanionDisplay aiDisplay;

    private string currentUserId;
    private string currentGender;
    private int currentCoins;
    private List<string> ownedItemIds = new List<string>();
    private string equippedItemId = "";
    private List<ItemCard> spawnedCards = new List<ItemCard>();

    void Awake()
    {
        currentUserId = PlayerPrefs.GetString("currentUserId", "");
        currentGender = PlayerPrefs.GetString("pendingGender", "انثى");

        if (characterDisplay != null)
        {
            characterDisplay.enabled = false;
        }
    }

    void Start()
    {
        LoadCachedDisplay();
        StartCoroutine(LoadUserData());
    }

    void LoadCachedDisplay()
    {
        equippedItemId = PlayerPrefs.GetString("LastEquipped_" + currentUserId, "");

        if (!string.IsNullOrEmpty(equippedItemId))
        {
            ApplyEquippedSprite(equippedItemId);
        }
        else
        {
            bool isGirl = currentGender == "أنثى" || currentGender == "انثى" || currentGender == "female";
            characterDisplay.sprite = isGirl ? girlSprite : boySprite;
            if (characterDisplay != null) characterDisplay.enabled = true;
        }

        InitializeStoreCards();
    }

    IEnumerator LoadUserData()
    {
        PerformanceLogger.Instance.StartMeasure("Store_Load");

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

                StartCoroutine(FirestoreManager.Instance.LoadOwnedItems(
                    currentUserId,
                    (items) => {
                        ownedItemIds.Clear();
                        foreach (var item in items)
                        {
                            if (item != null)
                            {
                                ownedItemIds.Add(item.itemId);
                                if (item.equipped) equippedItemId = item.itemId;
                            }
                        }

                        if (!string.IsNullOrEmpty(equippedItemId))
                        {
                            ApplyEquippedSprite(equippedItemId);
                        }
                        else
                        {
                            bool isGirl = currentGender == "أنثى" || currentGender == "انثى" || currentGender == "female";
                            characterDisplay.sprite = isGirl ? girlSprite : boySprite;
                            if (characterDisplay != null) characterDisplay.enabled = true;
                        }

                        PerformanceLogger.Instance.StopMeasure("Store_Load");
                        RefreshAllCards();
                    },
                     (error) => {
                         PerformanceLogger.Instance.StopMeasure("Store_Load"); 
                         Debug.LogError("Error fetching owned items: " + error);
                     }
                ));
            },
            (error) => {
                PerformanceLogger.Instance.StopMeasure("Store_Load"); 
                Debug.LogError("Failed to load user: " + error);
            }
        ));
    }

    void ApplyEquippedSprite(string itemId)
    {
        bool isGirl = currentGender == "أنثى" || currentGender == "انثى" || currentGender == "female";
        ItemData[] items = isGirl ? girlItems : boyItems;
        bool found = false;

        foreach (var item in items)
        {
            if (item != null && item.itemId == itemId)
            {
                characterDisplay.sprite = item.characterSprite;
                found = true;
                break;
            }
        }

        if (found)
        {
            if (characterDisplay != null)
            {
                characterDisplay.enabled = true;
            }
        }
        else
        {
            characterDisplay.sprite = isGirl ? girlSprite : boySprite;
            if (characterDisplay != null)
            {
                characterDisplay.enabled = true;
            }
        }
    }

    void InitializeStoreCards()
    {
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        spawnedCards.Clear();

        bool isGirl = currentGender == "أنثى" || currentGender == "انثى" || currentGender == "female";
        ItemData[] items = isGirl ? girlItems : boyItems;

        foreach (ItemData item in items)
        {
            if (item == null) continue;
            GameObject card = Instantiate(itemCardPrefab, itemsContainer);
            ItemCard itemCard = card.GetComponent<ItemCard>();
            itemCard.Setup(item, this);
            spawnedCards.Add(itemCard);
        }
    }

    void RefreshAllCards()
    {
        bool isGirl = currentGender == "أنثى" || currentGender == "انثى" || currentGender == "female";
        ItemData[] items = isGirl ? girlItems : boyItems;

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (i < items.Length && spawnedCards[i] != null)
            {
                spawnedCards[i].Setup(items[i], this);
            }
        }
    }

    public void BuyItem(ItemData item)
    {
        if (currentCoins < item.price)
        {
            Debug.LogWarning("Not enough coins!");
            return;
        }

        currentCoins -= item.price;
        ownedItemIds.Add(item.itemId);
        RefreshAllCards();

        StartCoroutine(FirestoreManager.Instance.UpdateCoins(
            currentUserId, currentCoins,
            () => {
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
            () => RefreshAllCards(),
            (error) => Debug.LogError(error)
        ));
    }

    public bool IsOwned(string itemId) => ownedItemIds.Contains(itemId);
    public bool IsEquipped(string itemId) => equippedItemId == itemId;

    public void EquipItem(ItemData item)
    {
        equippedItemId = item.itemId;
        characterDisplay.sprite = item.characterSprite;
        PlayerPrefs.SetString("LastEquipped_" + currentUserId, item.itemId);
        RefreshAllCards();
    }
}