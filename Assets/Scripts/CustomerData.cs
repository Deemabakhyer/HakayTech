using UnityEngine;

[System.Serializable]
public class CustomerData
{
    public string customerName;
    [TextArea(2, 5)] public string orderText;     // The text that goes into the speech bubble
    public Sprite customerSprite;                 // The visual sprite for this specific customer
    public RuntimeAnimatorController animatorCtrl; // The specific animator configuration for this customer

    [Header("Correct Solution Targets")]
    public BlockIdentity firstCondition;         // e.g., Khlas for Customer 1, Sukkari for Customer 2
    public BlockIdentity firstPrice;             // e.g., Price20 for Customer 1, Price30 for Customer 2
}