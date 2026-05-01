using UnityEngine;

[CreateAssetMenu(fileName = "StoryChallenge", menuName = "HakayTech/StoryChallenge")]
public class StoryChallengeData : ScriptableObject
{
    public string storyId;
    public string title;
    public string concept;
    public string region;
    public string description;
    public string difficultyLevel;
    public string goals;
    public string badgeId;
    public Sprite header;
}


[CreateAssetMenu(fileName = "Badge", menuName = "HakayTech/Badge")]
public class BadgeData : ScriptableObject
{
    public string badgeId;
    public string badgeName;
    public Sprite image;
}

[CreateAssetMenu(fileName = "AICompanion", menuName = "HakayTech/AICompanion")]
public class AICompanionData : ScriptableObject
{
    public string companionId;
    public string companionName;
    public string gender;
    public GameObject model3D;
    public RuntimeAnimatorController animationController;
}

[CreateAssetMenu(fileName = "BlockOfCode", menuName = "HakayTech/BlockOfCode")]
public class BlockOfCodeData : ScriptableObject
{
    public string blockId;
    public string blockName;
    public string type;
    public Sprite blockImage;
}