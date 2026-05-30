using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // أضيفي هذا فوق
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VariableManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_InputField inputField;
    public GameObject variablePrefab;
    public Transform container;
    public Image popupShellDisplay;
    public TMPro.TextMeshProUGUI validationText;

    [Header("Audio Clips")]
    public AudioClip beachSound;
    public AudioClip boxOpenSound;
    public AudioClip shellPickUpSound;
    private AudioSource myAudioSource;

    [Header("Shell Data")]
    public Sprite[] shellSprites;
    private int currentShellID;

    [Header("Girl Animation")]
    public Animator girlAnimator; 

    private Dictionary<string, int> savedVariables = new Dictionary<string, int>();

    [Header("Final Block")]
    public GameObject variableBlockPrefab;

    [Header("Shells On Beach")]
    public GameObject[] beachShells;

    [Header("Magic Box")]
    public Transform magicBox; 

    [Header("Submit & Reveal")]
    public Transform shellSpawnPoint;
    public float delayBetweenShells = 1f;

    [Header("Success UI")]
    public CompletionPopupController completionPopup; 


    void Start()
    {
        popupPanel.SetActive(false);
        myAudioSource = GetComponent<AudioSource>();

        if (beachSound != null)
        {
            myAudioSource.clip = beachSound;
            myAudioSource.loop = true;
            myAudioSource.Play();
        }
    }

    public Dictionary<string, int> GetSavedVariables()
    {
        return savedVariables;
    }


    public void PlayBoxOpenEffect()
    {
        if (boxOpenSound != null) myAudioSource.PlayOneShot(boxOpenSound);
        Debug.Log("Box sound called");

    }

    public void PlayShellSound()
    {
        if (shellPickUpSound != null)
            myAudioSource.PlayOneShot(shellPickUpSound);

        MoveShellToBox(currentShellID);
    }

    public void StartShellInteraction(int shellID)
    {
        currentShellID = shellID;

        if (beachShells != null && beachShells.Length > shellID)
            beachShells[shellID].SetActive(false);

        if (girlAnimator != null)
            girlAnimator.speed = 0f;

        if (shellPickUpSound != null)
            myAudioSource.PlayOneShot(shellPickUpSound);

        if (popupShellDisplay != null && shellSprites != null && shellSprites.Length > shellID)
        {
            popupShellDisplay.sprite = shellSprites[shellID];
            popupShellDisplay.enabled = true;
        }

        OpenPopup();
    }



    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void SaveVariable()
    {
        string varName = inputField.text.Trim();

        if (string.IsNullOrEmpty(varName))
        {
            validationText.text = "يا بطل، اكتب اسم للصدفة أولاً!";
            return; 
        }

        if (char.IsDigit(varName[0]))
        {
            validationText.text = "خطأ: اسم المتغير لا يمكن أن يبدأ برقم!";
            return; 
        }
        if (varName.Contains(" "))
        {
            validationText.text = "خطأ: لا تستخدم المسافات في اسم المتغير.";
            return; 
        }

        if (savedVariables.ContainsKey(varName))
        {
            validationText.text = "هذا الاسم محجوز لصدفة أخرى، اختر اسماً جديداً.";
            return; 
        }

        if (container.childCount < 3)
        {

            savedVariables.Add(varName, currentShellID);
            Debug.Log($"تم الحفظ: {varName} مرتبطة بالصدفة رقم {currentShellID}");

            if (beachShells != null && beachShells.Length > currentShellID)
            {
                beachShells[currentShellID].SetActive(true);
            }

            GameObject newVar = Instantiate(variablePrefab, container, false);

            TextMeshProUGUI textComp = newVar.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = varName;
            }

            if (container.childCount == 3)
            {
                CreateFinalVariableBlock();
            }

            validationText.text = "";
            ClosePopup();
        }
        else
        {
            validationText.text = "لقد جمعت كل الأصداف المسموحة!";
        }
    }



    public void ClosePopup()
    {
        popupPanel.SetActive(false);

        if (girlAnimator != null)
            girlAnimator.speed = 1f;
    }


    public void MagicBoxExtract(string chosenName)
    {
        if (savedVariables.ContainsKey(chosenName))
        {
            int shellID = savedVariables[chosenName];
            Debug.Log("الصندوق السحري يستخرج الصدفة رقم: " + shellID);
            TriggerMagicBoxAnimation(shellID);
        }
    }

    void TriggerMagicBoxAnimation(int shellID)
    {
        Debug.Log("جاري تشغيل أنميشن الصندوق للصدفة: " + shellID);
    }

    void CreateFinalVariableBlock()
    {
        if (variableBlockPrefab != null)
        {
            Instantiate(variableBlockPrefab, container, false);
        }
    }

    public void MoveShellToBox(int shellID)
    {
        if (beachShells == null || beachShells.Length <= shellID) return;

        GameObject shell = beachShells[shellID];
        if (shell == null) return;

        shell.transform
            .DOMove(magicBox.position, 0.8f)
            .SetEase(Ease.InBack)
            .OnComplete(() => shell.SetActive(false));
            Debug.Log("MoveShellToBox called: " + shellID);

    }


    private List<VariableBlock> placedBlocks = new List<VariableBlock>();

    public void RegisterPlacedBlock(VariableBlock block)
    {
        foreach (var b in placedBlocks)
        {
            if (b.GetInstanceID() == block.GetInstanceID())
            {
                Debug.LogWarning("نفس البلوك موجود!");
                return;
            }
        }
        placedBlocks.Add(block);
        Debug.Log("تسجّل: " + block.GetSelectedName() + " | إجمالي: " + placedBlocks.Count);
    }
    public void OnSubmit()
    {
        Debug.Log("OnSubmit نودي عليها - عدد البلوكات: " + placedBlocks.Count);

        if (placedBlocks.Count == 0)
        {
            Debug.LogWarning("ما في بلوكات في الحل!");
            return;
        }

        List<int> order = new List<int>();
        foreach (var block in placedBlocks)
        {
            Debug.Log("بلوك: " + block.GetSelectedName() + " - ID: " + block.GetSelectedShellID());
            order.Add(block.GetSelectedShellID());
        }

        StartCoroutine(RevealShells(order));
    }




    IEnumerator RevealShells(List<int> order)
    {
        float spacing = 1.5f; 
        int index = 0;

        foreach (int shellID in order)
        {
            if (shellID < 0 || shellID >= beachShells.Length) continue;

            GameObject shell = beachShells[shellID];
            if (shell == null) continue;

            shell.SetActive(true);
            shell.transform.position = magicBox.position;

            Vector3 targetPos = shellSpawnPoint.position + new Vector3(index * spacing, 0, 0);

            shell.transform
                .DOMove(targetPos, 0.8f)
                .SetEase(Ease.OutBack);

            index++;
            yield return new WaitForSeconds(delayBetweenShells);
        }

        yield return new WaitForSeconds(1.0f);

        if (completionPopup != null)
        {
            completionPopup.ShowPopup();
        }
    }




    [Header("Celebration Sound")]
    public AudioClip celebrationSound;

    public void PlayCelebrationSound()
    {
        if (celebrationSound != null)
            myAudioSource.PlayOneShot(celebrationSound);
            myAudioSource.PlayOneShot(celebrationSound, 1f); 
    }


}








