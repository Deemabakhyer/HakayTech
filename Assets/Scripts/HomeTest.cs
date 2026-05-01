using UnityEngine;
using TMPro;
using System.Collections;

public class HomeTest : MonoBehaviour
{
    public TMP_Text nameText;

    void Start()
    {
        string userId = PlayerPrefs.GetString("currentUserId");

        if (string.IsNullOrEmpty(userId))
        {
            nameText.text = "No User";
            return;
        }

        StartCoroutine(FirestoreManager.Instance.LoadUser(
            userId,
            (user) =>
            {
                nameText.text = "Hello " + user.name;
            },
            (error) =>
            {
                nameText.text = "Error: " + error;
            }
        ));
    }
}