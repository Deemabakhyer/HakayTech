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