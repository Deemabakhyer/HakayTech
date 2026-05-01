using UnityEngine;

using UnityEngine.UI;

using TMPro;

using System.Collections;

using System.Collections.Generic;



public class StoreManager : MonoBehaviour

{

    [Header("Character Display")]

    public Image characterDisplay;

    public Sprite boySprite;

    public Sprite girlSprite;



    [Header("Items")]

    public ItemData[] boyItems;   // اسحبي أيتمز الولد

    public ItemData[] girlItems;  // اسحبي أيتمز البنت

    public Transform itemsContainer;

    public GameObject itemCardPrefab;



    [Header("Coins")]

    public TMP_Text coinsText;



    private string currentUserId;

    private string currentGender;

    private int currentCoins;

    private List<string> ownedItemIds = new List<string>();

    private string equippedItemId = "";



    void Start()

    {

        currentUserId = PlayerPrefs.GetString("currentUserId");

        Debug.Log("User ID: " + currentUserId);

        StartCoroutine(LoadUserData());

    }

    IEnumerator LoadUserData()
    {
        yield return StartCoroutine(FirestoreManager.Instance.LoadUser(
        currentUserId,
        (user) =>
        {
            currentGender = user.gender;
            currentCoins = user.accumulatedCoins;
            coinsText.text = currentCoins.ToString();
            SetCharacter(currentGender);

            // 1. استرجاع الزي المجهز من ذاكرة الجهاز فوراً
            equippedItemId = PlayerPrefs.GetString("LastEquipped_" + currentUserId, "");

            // 2. جلب المشتريات من Firestore
            StartCoroutine(FirestoreManager.Instance.LoadOwnedItems(
                currentUserId,
                (items) => {
                ownedItemIds.Clear();
                foreach (var item in items)
                {
                    ownedItemIds.Add(item.itemId);

                        // إذا كان السيرفر يقول أن هذا العنصر مجهز، نحدث القيمة
                        if (item.equipped) equippedItemId = item.itemId;
        }

                    // 3. الآن بعد أن عرفنا الـ ID المجهز، نغير الصورة ونعرض الكاردات
                    if (!string.IsNullOrEmpty(equippedItemId))
        {
            ApplyEquippedSprite(equippedItemId);
        }

        LoadItems();
    },
                (error) => Debug.LogError("خطأ في جلب المشتريات: " + error)
            ));
        },
        (error) => Debug.LogError("Failed to load user: " + error)
    ));

    }

    void ApplyEquippedSprite(string itemId)
    {
        // نحدد أي مصفوفة نبحث فيها بناءً على الجنس
        bool isGirl = currentGender == "أنثى" || currentGender == "female";
        ItemData[] items = isGirl ? girlItems : boyItems;

        foreach (var item in items)
        {
            if (item.itemId == itemId)
            {
                // تحديث سبرايت الروبوت بالزي المجهز
                characterDisplay.sprite = item.characterSprite;
                break;
            }
        }
    }



    void SetCharacter(string gender)

    {

        bool isGirl = gender == "أنثى" || gender == "female";

        characterDisplay.sprite = isGirl ? girlSprite : boySprite;

    }



    void LoadItems()

    {

        // امسحي الكاردات القديمة

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



    public void BuyItem(ItemData item)

    {

        if (currentCoins < item.price)

        {

            Debug.LogWarning("ما يكفي كوينز!");

            return;

        }



        currentCoins -= item.price;

        coinsText.text = currentCoins.ToString();

        ownedItemIds.Add(item.itemId);



        // احفظي في Firestore

        StartCoroutine(FirestoreManager.Instance.UpdateCoins(

            currentUserId, currentCoins,

            () => Debug.Log("Coins updated!"),

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
    public void EquipItem(ItemData item)
    {
        equippedItemId = item.itemId;
        characterDisplay.sprite = item.characterSprite;

      PlayerPrefs.SetString("LastEquipped_" + currentUserId, item.itemId);


        LoadItems();
    }

    IEnumerator UpdateEquippedStateInDatabase(string selectedItemId)
    {

        Debug.Log("جاري حفظ اختيار الزي في الداتابيس..."); 
    yield return null;
    }



}