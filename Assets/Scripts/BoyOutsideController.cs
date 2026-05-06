using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class BoyOutsideController : MonoBehaviour
{
    [Header("Intro States")]
    public GameObject state01_PressButtons;
    public GameObject state02_ShockedAI;
    public GameObject state03_ShockedAgain;
    public GameObject state04_WelcomeHologram;
    public GameObject state05_HappyReady;

    [Header("Gameplay States")]
    public GameObject state06_PressA;
    public GameObject state07_PressB;
    public GameObject state08_ThinkA;
    public GameObject state09_ThinkB;
    public GameObject state10_ThinkC;
    public GameObject state11_ThinkD;
    public GameObject state12_FixPressA;
    public GameObject state13_FixPressB;
    public GameObject state14_FixPressC;
    public GameObject state15_Celebrate;

    [Header("Linked Screen Controller")]
    public ScreenStateVisual screenStateVisual;

    [Header("Preview (Editor Only)")]
    public bool previewMode = false;
    [Range(1, 15)] public int previewState = 1;

    [Header("Intro Timing")]
    public bool playIntroOnStart = true;
    public float pressDuration = 1.5f;
    public float shockDuration = 1.0f;
    public float secondShockDuration = 1.0f;
    public float welcomeDuration = 1.0f;

    private Coroutine introRoutine;

    void OnValidate()
    {
        if (!Application.isPlaying && previewMode)
        {
            ShowState(previewState);

            if (screenStateVisual != null)
                screenStateVisual.ShowState(previewState);
        }
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        if (playIntroOnStart)
            PlayIntro();
        else
        {
            ShowState(1);
            if (screenStateVisual != null)
                screenStateVisual.ShowState(1);
        }
    }

    public void PlayIntro()
    {
        if (introRoutine != null)
            StopCoroutine(introRoutine);

        introRoutine = StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        if (screenStateVisual != null) screenStateVisual.ShowState(1);
        ShowState(1);
        yield return new WaitForSeconds(pressDuration);

        if (screenStateVisual != null) screenStateVisual.ShowState(2);
        ShowState(2);
        yield return new WaitForSeconds(shockDuration);

        if (screenStateVisual != null) screenStateVisual.ShowState(3);
        ShowState(3);
        yield return new WaitForSeconds(secondShockDuration);

        if (screenStateVisual != null) screenStateVisual.ShowState(4);
        ShowState(4);
        yield return new WaitForSeconds(welcomeDuration);

        if (screenStateVisual != null) screenStateVisual.ShowState(5);
        ShowState(5);

        introRoutine = null;
    }

    public void ShowState(int index)
    {
        HideAllStates();

        switch (index)
        {
            case 1: if (state01_PressButtons) state01_PressButtons.SetActive(true); break;
            case 2: if (state02_ShockedAI) state02_ShockedAI.SetActive(true); break;
            case 3: if (state03_ShockedAgain) state03_ShockedAgain.SetActive(true); break;
            case 4: if (state04_WelcomeHologram) state04_WelcomeHologram.SetActive(true); break;
            case 5: if (state05_HappyReady) state05_HappyReady.SetActive(true); break;
            case 6: if (state06_PressA) state06_PressA.SetActive(true); break;
            case 7: if (state07_PressB) state07_PressB.SetActive(true); break;
            case 8: if (state08_ThinkA) state08_ThinkA.SetActive(true); break;
            case 9: if (state09_ThinkB) state09_ThinkB.SetActive(true); break;
            case 10: if (state10_ThinkC) state10_ThinkC.SetActive(true); break;
            case 11: if (state11_ThinkD) state11_ThinkD.SetActive(true); break;
            case 12: if (state12_FixPressA) state12_FixPressA.SetActive(true); break;
            case 13: if (state13_FixPressB) state13_FixPressB.SetActive(true); break;
            case 14: if (state14_FixPressC) state14_FixPressC.SetActive(true); break;
            case 15: if (state15_Celebrate) state15_Celebrate.SetActive(true); break;
        }
    }

    void HideAllStates()
    {
        if (state01_PressButtons) state01_PressButtons.SetActive(false);
        if (state02_ShockedAI) state02_ShockedAI.SetActive(false);
        if (state03_ShockedAgain) state03_ShockedAgain.SetActive(false);
        if (state04_WelcomeHologram) state04_WelcomeHologram.SetActive(false);
        if (state05_HappyReady) state05_HappyReady.SetActive(false);
        if (state06_PressA) state06_PressA.SetActive(false);
        if (state07_PressB) state07_PressB.SetActive(false);
        if (state08_ThinkA) state08_ThinkA.SetActive(false);
        if (state09_ThinkB) state09_ThinkB.SetActive(false);
        if (state10_ThinkC) state10_ThinkC.SetActive(false);
        if (state11_ThinkD) state11_ThinkD.SetActive(false);
        if (state12_FixPressA) state12_FixPressA.SetActive(false);
        if (state13_FixPressB) state13_FixPressB.SetActive(false);
        if (state14_FixPressC) state14_FixPressC.SetActive(false);
        if (state15_Celebrate) state15_Celebrate.SetActive(false);
    }
}