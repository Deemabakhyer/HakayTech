using UnityEngine;

[CreateAssetMenu(fileName = "AICompanion", menuName = "HakayTech/AICompanion")]
public class AICompanionData : ScriptableObject
{
    public string companionId;
    public string companionName;
    public string gender;
    public GameObject model3D;
    public RuntimeAnimatorController animationController;
}
