using System.Collections.Generic;

// ========= Dynamic (Firestore) =========

[System.Serializable]
public class UserGameData
{
    public string userId;
    public string name;
    public string email;
    public int age;
    public string grade;
    public string gender;
    public string avatar;
    public string aiCompanionId;
    public int accumulatedCoins;
    public List<string> earnedBadges = new List<string>();
    public List<ProgressData> progress;
    public List<OwnedItem> ownedItems;
}


[System.Serializable]
public class ProgressData
{
    public string progressId;
    public string userId;
    public string storyChallengeId;
    public int coins;
    public string state; // "locked" | "available" | "completed"
}

[System.Serializable]
public class SessionData
{
    public string sessionId;
    public string userId;
    public string startTime;
    public string endTime;
    public float time;
}

[System.Serializable]
public class OwnedItem
{
    public string ownedItemId;
    public string userId;
    public string itemId;
    public bool equipped;
}