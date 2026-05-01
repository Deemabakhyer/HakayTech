using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    string BaseUrl => $"https://firestore.googleapis.com/v1/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents";

    // ========= USER =========

    public IEnumerator SaveUser(UserGameData user,
    System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/users/{user.userId}";

        var badgesList = new List<object>();
        if (user.earnedBadges != null)
            foreach (var b in user.earnedBadges)
                badgesList.Add(new Dictionary<string, object> { { "stringValue", b } });

        var fields = new Dictionary<string, object>
    {
        { "fields", new Dictionary<string, object>
            {
                { "name",             StringField(user.name) },
                { "email",            StringField(user.email) },
                { "age",              IntField(user.age) },
                { "gender",           StringField(user.gender) },
                { "grade",            StringField(user.grade) },
                { "avatar",           StringField(user.avatar ?? "") },
                { "aiCompanionId",    StringField(user.aiCompanionId ?? "") },
                { "accumulatedCoins", IntField(user.accumulatedCoins) },
                { "earnedBadges",     new Dictionary<string, object>
                    { { "arrayValue", new Dictionary<string, object>
                        { { "values", badgesList } }
                    }}
                }
            }
        }
    };

        yield return Patch(url, fields, onSuccess, onError);
    }

    public IEnumerator LoadUser(string userId,
        System.Action<UserGameData> onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/users/{userId}";

        yield return Get(url, (response) =>
        {
            var fields = ParseFields(response);
            UserGameData user = new UserGameData
            {
                userId = userId,
                name = GetString(fields, "name"),
                email = GetString(fields, "email"),
                gender = GetString(fields, "gender"),
                grade = GetString(fields, "grade"),
                avatar = GetString(fields, "avatar"),
                aiCompanionId = GetString(fields, "aiCompanionId"),
                accumulatedCoins = GetInt(fields, "accumulatedCoins")
            };
            onSuccess?.Invoke(user);
        }, onError);
    }

    public IEnumerator UpdateCoins(string userId, int coins,
        System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/users/{userId}?updateMask.fieldPaths=accumulatedCoins";

        var fields = new Dictionary<string, object>
        {
            { "fields", new Dictionary<string, object>
                { { "accumulatedCoins", IntField(coins) } }
            }
        };

        yield return Patch(url, fields, onSuccess, onError);
    }

    public IEnumerator UpdateAvatar(string userId, string avatar,
        System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/users/{userId}?updateMask.fieldPaths=avatar";

        var fields = new Dictionary<string, object>
        {
            { "fields", new Dictionary<string, object>
                { { "avatar", StringField(avatar) } }
            }
        };

        yield return Patch(url, fields, onSuccess, onError);
    }

    // ========= PROGRESS =========

    public IEnumerator SaveProgress(ProgressData progress,
        System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/progress/{progress.progressId}";

        var fields = new Dictionary<string, object>
        {
            { "fields", new Dictionary<string, object>
                {
                    { "userId",           StringField(progress.userId) },
                    { "storyChallengeId", StringField(progress.storyChallengeId) },
                    { "coins",            IntField(progress.coins) },
                    { "state",            StringField(progress.state) }
                }
            }
        };

        yield return Patch(url, fields, onSuccess, onError);
    }

    public IEnumerator LoadUserProgress(string userId,
        System.Action<List<ProgressData>> onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/progress?pageSize=100";

        yield return Get(url, (response) =>
        {
            List<ProgressData> progressList = new List<ProgressData>();
            if (response.Contains(userId))
            {
                // Parse كل document
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                onSuccess?.Invoke(progressList);
            }
            else onSuccess?.Invoke(progressList);
        }, onError);
    }

    // ========= SESSION =========

    public IEnumerator SaveSession(SessionData session,
        System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/sessions/{session.sessionId}";

        var fields = new Dictionary<string, object>
        {
            { "fields", new Dictionary<string, object>
                {
                    { "userId",    StringField(session.userId) },
                    { "startTime", StringField(session.startTime) },
                    { "endTime",   StringField(session.endTime) },
                    { "time",      IntField((int)session.time) }
                }
            }
        };

        yield return Patch(url, fields, onSuccess, onError);
    }

    // ========= OWNED ITEMS =========

    public IEnumerator SaveOwnedItem(OwnedItem item,
        System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/ownedItems/{item.ownedItemId}";

        var fields = new Dictionary<string, object>
        {
            { "fields", new Dictionary<string, object>
                {
                    { "userId",   StringField(item.userId) },
                    { "itemId",   StringField(item.itemId) },
                    { "equipped", BoolField(item.equipped) }
                }
            }
        };

        yield return Patch(url, fields, onSuccess, onError);
    }

    public IEnumerator UpdateEquip(string ownedItemId, bool equipped,
        System.Action onSuccess, System.Action<string> onError)
    {
        string url = $"{BaseUrl}/ownedItems/{ownedItemId}?updateMask.fieldPaths=equipped";

        var fields = new Dictionary<string, object>
        {
            { "fields", new Dictionary<string, object>
                { { "equipped", BoolField(equipped) } }
            }
        };

        yield return Patch(url, fields, onSuccess, onError);
    }

    // ========= HELPERS =========

    Dictionary<string, object> StringField(string val) =>
        new Dictionary<string, object> { { "stringValue", val } };

    Dictionary<string, object> IntField(int val) =>
        new Dictionary<string, object> { { "integerValue", val } };

    Dictionary<string, object> BoolField(bool val) =>
        new Dictionary<string, object> { { "booleanValue", val } };

    Dictionary<string, Dictionary<string, object>> ParseFields(string response)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
        return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(
            data["fields"].ToString());
    }

    string GetString(Dictionary<string, Dictionary<string, object>> fields, string key) =>
        fields.ContainsKey(key) ? fields[key]["stringValue"].ToString() : "";

    int GetInt(Dictionary<string, Dictionary<string, object>> fields, string key) =>
        fields.ContainsKey(key) ? int.Parse(fields[key]["integerValue"].ToString()) : 0;

    IEnumerator Patch(string url, object body,
        System.Action onSuccess, System.Action<string> onError)
    {
        string json = JsonConvert.SerializeObject(body);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke();
            else
                onError?.Invoke(request.downloadHandler.text);
        }
    }

    IEnumerator Get(string url, System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text);
        }
    }


    public IEnumerator LoadOwnedItems(string userId, System.Action<List<OwnedItem>> onSuccess, System.Action<string> onError)
    {
        // الرابط الصحيح لجلب الوثائق من كولكشن ownedItems
        string url = $"{BaseUrl}/ownedItems?pageSize=100";

        yield return Get(url, (response) =>
        {
            List<OwnedItem> ownedList = new List<OwnedItem>();

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                if (data.ContainsKey("documents"))
                {
                    var documents = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["documents"].ToString());
                    foreach (var doc in documents)
                    {
                        var fields = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(doc["fields"].ToString());

                        // نتحقق إذا كانت الوثيقة تخص المستخدم الحالي
                        if (GetString(fields, "userId") == userId)
                        {
                            ownedList.Add(new OwnedItem
                            {
                                ownedItemId = doc["name"].ToString().Split('/')[^1],
                                userId = GetString(fields, "userId"),
                                itemId = GetString(fields, "itemId"),
                                equipped = fields.ContainsKey("equipped") ? bool.Parse(fields["equipped"]["booleanValue"].ToString()) : false
                            });
                        }
                    }
                }
                onSuccess?.Invoke(ownedList);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing owned items: " + e.Message);
                onSuccess?.Invoke(ownedList);
            }
        }, onError);
    }



}