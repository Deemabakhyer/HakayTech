using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryData", menuName = "AI/Story Data")]
public class StoryData : ScriptableObject
{
    public string storyKey;
    public string playerGender = "male";
    public List<string> correctBlockOrder = new List<string>();
}
