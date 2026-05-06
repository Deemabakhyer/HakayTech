using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ScreenStateVisual : MonoBehaviour
{
    [Header("Target Screen Image")]
    public Image targetImage;

    [Header("Scene Sprites (15 States)")]
    public Sprite state01Sprite, state02Sprite, state03Sprite, state04Sprite, state05Sprite;
    public Sprite state06Sprite, state07Sprite, state08Sprite, state09Sprite, state10Sprite;
    public Sprite state11Sprite, state12Sprite, state13Sprite, state14Sprite, state15Sprite;

    [Header("Linked Boy Controller")]
    public BoyOutsideController boyController;

    [Header("Player Spawn Intro")]
    public PlayerSpawnIntro playerSpawnIntro;
    public bool spawnPlayerOnState5 = true;
    private bool playerSpawned = false;

    [Header("Intro In-Screen AI")]
    public GameObject introAIMentor;
    public GameObject introDialogueBubble;
    public int hideIntroAIFromState = 4;

    [Header("Boy State Mapping")]
    public int boyState01 = 1, boyState02 = 2, boyState03 = 3, boyState04 = 4, boyState05 = 5;
    public int boyState06 = 6, boyState07 = 7, boyState08 = 8, boyState09 = 9, boyState10 = 10;
    public int boyState11 = 11, boyState12 = 12, boyState13 = 13, boyState14 = 14, boyState15 = 15;

    [Header("Auto Transition")]
    public bool autoState09To10 = true;
    public float state09To10Delay = 0.9f;

    [Header("Legacy Mapping")]
    public int normalState = 1;
    public int errorState = 8;
    public int recoverState = 12;
    public int successGlowState = 14;
    public int completedState = 15;

    [Header("Preview")]
    public bool previewOnStart = false;
    [Range(1, 15)] public int previewState = 1;

    public int CurrentState { get; private set; } = 1;

    void OnValidate()
    {
        if (!Application.isPlaying && previewOnStart)
            ShowState(previewState);
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        if (previewOnStart)
            ShowState(previewState);
    }

    public void ShowState(int index)
    {
        CurrentState = index;

        if (Application.isPlaying)
            CancelInvoke(nameof(AutoGoToState10));

        if (targetImage == null)
            return;

        Sprite spriteToShow = GetSpriteByState(index);

        if (spriteToShow != null)
            targetImage.sprite = spriteToShow;

        UpdateIntroAIVisibility(index);
        TrySpawnPlayer(index);

        if (boyController != null)
            boyController.ShowState(GetBoyStateByScreenState(index));

        // مهم: فقط State 9 ينتقل إلى State 10
        // State 8 لا ينتقل تلقائيًا أبدًا
        if (Application.isPlaying && autoState09To10 && index == 9)
            Invoke(nameof(AutoGoToState10), state09To10Delay);
    }

    void UpdateIntroAIVisibility(int index)
    {
        bool showIntroAI = index < hideIntroAIFromState;

        if (introAIMentor != null)
            introAIMentor.SetActive(showIntroAI);

        if (introDialogueBubble != null)
            introDialogueBubble.SetActive(showIntroAI);
    }

    void TrySpawnPlayer(int index)
    {
        if (!Application.isPlaying) return;
        if (!spawnPlayerOnState5) return;
        if (playerSpawned) return;

        if (index == 5 && playerSpawnIntro != null)
        {
            playerSpawnIntro.PlaySpawn();
            playerSpawned = true;
        }
    }

    void AutoGoToState10()
    {
        ShowState(10);
    }

    private Sprite GetSpriteByState(int index)
    {
        switch (index)
        {
            case 1: return state01Sprite;
            case 2: return state02Sprite;
            case 3: return state03Sprite;
            case 4: return state04Sprite;
            case 5: return state05Sprite;
            case 6: return state06Sprite;
            case 7: return state07Sprite;
            case 8: return state08Sprite;
            case 9: return state09Sprite;
            case 10: return state10Sprite;
            case 11: return state11Sprite;
            case 12: return state12Sprite;
            case 13: return state13Sprite;
            case 14: return state14Sprite;
            case 15: return state15Sprite;
            default: return null;
        }
    }

    private int GetBoyStateByScreenState(int screenState)
    {
        switch (screenState)
        {
            case 1: return boyState01;
            case 2: return boyState02;
            case 3: return boyState03;
            case 4: return boyState04;
            case 5: return boyState05;
            case 6: return boyState06;
            case 7: return boyState07;
            case 8: return boyState08;
            case 9: return boyState09;
            case 10: return boyState10;
            case 11: return boyState11;
            case 12: return boyState12;
            case 13: return boyState13;
            case 14: return boyState14;
            case 15: return boyState15;
            default: return 1;
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

    public void ShowNormal() { ShowState(normalState); }
    public void ShowError() { ShowState(errorState); }
    public void ShowRecover() { ShowState(recoverState); }
    public void ShowSuccessGlow() { ShowState(successGlowState); }
    public void ShowCompletedPath() { ShowState(completedState); }
}