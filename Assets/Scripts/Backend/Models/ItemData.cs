using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "HakayTech/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite itemImage;       
    public Sprite characterSprite;
    public int price;
}
