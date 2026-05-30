using UnityEngine;

public class AIDialogueController : MonoBehaviour
{
    [Header("Top AI States (15 States)")]
    public GameObject state01;
    public GameObject state02;
    public GameObject state03;
    public GameObject state04;
    public GameObject state05;
    public GameObject state06;
    public GameObject state07;
    public GameObject state08;
    public GameObject state09;
    public GameObject state10;
    public GameObject state11;
    public GameObject state12;
    public GameObject state13;
    public GameObject state14;
    public GameObject state15;

    [Header("Optional In-Screen Popups")]
    public GameObject screenErrorPopup;
    public GameObject screenSuccessPopup;

    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip errorClip;
    public AudioClip successClip;

    [Header("Default States Mapping")]
    [Range(1, 15)] public int startState = 1;
    [Range(1, 15)] public int errorState = 7;
    [Range(1, 15)] public int successState = 15;

    void Start()
    {
        ShowState(startState);
    }

    public void ShowState(int index)
    {
        HideAllStates();

        switch (index)
        {
            case 1:  if (state01 != null) state01.SetActive(true); break;
            case 2:  if (state02 != null) state02.SetActive(true); break;
            case 3:  if (state03 != null) state03.SetActive(true); break;
            case 4:  if (state04 != null) state04.SetActive(true); break;
            case 5:  if (state05 != null) state05.SetActive(true); break;
            case 6:  if (state06 != null) state06.SetActive(true); break;
            case 7:  if (state07 != null) state07.SetActive(true); break;
            case 8:  if (state08 != null) state08.SetActive(true); break;
            case 9:  if (state09 != null) state09.SetActive(true); break;
            case 10: if (state10 != null) state10.SetActive(true); break;
            case 11: if (state11 != null) state11.SetActive(true); break;
            case 12: if (state12 != null) state12.SetActive(true); break;
            case 13: if (state13 != null) state13.SetActive(true); break;
            case 14: if (state14 != null) state14.SetActive(true); break;
            case 15: if (state15 != null) state15.SetActive(true); break;
            default:
                Debug.LogWarning("AIDialogueController: invalid state index = " + index);
                break;
        }
    }

    public void ShowState01() { ShowState(1); }
    public void ShowState02() { ShowState(2); }
    public void ShowState03() { ShowState(3); }
    public void ShowState04() { ShowState(4); }
    public void ShowState05() { ShowState(5); }
    public void ShowState06() { ShowState(6); }
    public void ShowState07() { ShowState(7); }
    public void ShowState08() { ShowState(8); }
    public void ShowState09() { ShowState(9); }
    public void ShowState10() { ShowState(10); }
    public void ShowState11() { ShowState(11); }
    public void ShowState12() { ShowState(12); }
    public void ShowState13() { ShowState(13); }
    public void ShowState14() { ShowState(14); }
    public void ShowState15() { ShowState(15); }

    public void ShowStartMessage()
    {
        ShowState(startState);
    }

    public void ShowErrorMessage()
    {
        ShowState(errorState);

        if (screenSuccessPopup != null)
            screenSuccessPopup.SetActive(false);

        if (screenErrorPopup != null)
        {
            CancelInvoke(nameof(HideErrorPopup));
            screenErrorPopup.SetActive(true);
            Invoke(nameof(HideErrorPopup), 1.2f);
        }

        if (audioSource != null && errorClip != null)
            audioSource.PlayOneShot(errorClip);
    }

    public void ShowSuccessMessage()
    {
        ShowState(successState);

        if (screenErrorPopup != null)
            screenErrorPopup.SetActive(false);

        if (screenSuccessPopup != null)
        {
            CancelInvoke(nameof(HideSuccessPopup));
            screenSuccessPopup.SetActive(true);
            Invoke(nameof(HideSuccessPopup), 1.2f);
        }

        if (audioSource != null && successClip != null)
            audioSource.PlayOneShot(successClip);
    }

    public void ShowErrorPopup(float duration = 1.2f)
    {
        if (screenSuccessPopup != null)
            screenSuccessPopup.SetActive(false);

        if (screenErrorPopup != null)
        {
            CancelInvoke(nameof(HideErrorPopup));
            screenErrorPopup.SetActive(true);
            Invoke(nameof(HideErrorPopup), duration);
        }

        if (audioSource != null && errorClip != null)
            audioSource.PlayOneShot(errorClip);
    }

    public void ShowSuccessPopup(float duration = 1.2f)
    {
        if (screenErrorPopup != null)
            screenErrorPopup.SetActive(false);

        if (screenSuccessPopup != null)
        {
            CancelInvoke(nameof(HideSuccessPopup));
            screenSuccessPopup.SetActive(true);
            Invoke(nameof(HideSuccessPopup), duration);
        }

        if (audioSource != null && successClip != null)
            audioSource.PlayOneShot(successClip);
    }

    void HideErrorPopup()
    {
        if (screenErrorPopup != null)
            screenErrorPopup.SetActive(false);
    }

    void HideSuccessPopup()
    {
        if (screenSuccessPopup != null)
            screenSuccessPopup.SetActive(false);
    }

    public void HideAllStates()
    {
        if (state01 != null) state01.SetActive(false);
        if (state02 != null) state02.SetActive(false);
        if (state03 != null) state03.SetActive(false);
        if (state04 != null) state04.SetActive(false);
        if (state05 != null) state05.SetActive(false);
        if (state06 != null) state06.SetActive(false);
        if (state07 != null) state07.SetActive(false);
        if (state08 != null) state08.SetActive(false);
        if (state09 != null) state09.SetActive(false);
        if (state10 != null) state10.SetActive(false);
        if (state11 != null) state11.SetActive(false);
        if (state12 != null) state12.SetActive(false);
        if (state13 != null) state13.SetActive(false);
        if (state14 != null) state14.SetActive(false);
        if (state15 != null) state15.SetActive(false);
    }
}