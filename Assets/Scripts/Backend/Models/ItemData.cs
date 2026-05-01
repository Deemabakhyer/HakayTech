
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "HakayTech/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite itemImage;        // صورة الزي للكارد
    public Sprite characterSprite;  // صورة الشخصية لابسة الزي
    public int price;
}
