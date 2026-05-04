using UnityEngine;

public class HomeMenuController : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject storePanel;
    public GameObject profilePanel;

    public void OpenHome()
    {
        homePanel.SetActive(true);
        storePanel.SetActive(false);
        profilePanel.SetActive(false);
    }

    public void OpenStore()
    {
        homePanel.SetActive(false);
        storePanel.SetActive(true);
        profilePanel.SetActive(false);
    }

    public void OpenProfile()
    {
        homePanel.SetActive(false);
        storePanel.SetActive(false);
        profilePanel.SetActive(true);
    }
}