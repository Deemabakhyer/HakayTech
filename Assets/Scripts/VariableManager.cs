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
    public Animator girlAnimator; // اسحبي أوبجكت الطفلة (child) هنا في الـ Inspector

    private Dictionary<string, int> savedVariables = new Dictionary<string, int>();

    [Header("Final Block")]
    public GameObject variableBlockPrefab;

    [Header("Shells On Beach")]
    public GameObject[] beachShells;

    [Header("Magic Box")]
    public Transform magicBox; // اسحبي الصندوق هنا

    [Header("Submit & Reveal")]
    public Transform shellSpawnPoint;
    public float delayBetweenShells = 1f;

    [Header("Success UI")]
    public CompletionPopupController completionPopup; // اسحبي كائن البوب أب هنا في الانسبكتور


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

    // --- 1. دوال الـ Animation Events ---

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

        // 👇 اخفاء الصدفة من الأرض
        if (beachShells != null && beachShells.Length > shellID)
            beachShells[shellID].SetActive(false);

        // إيقاف الأنيمشن عند فتح البوب اب
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


    // --- 2. نظام التسمية والحفظ ---

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void SaveVariable()
    {
        // 1. تنظيف النص من المسافات الزائدة في البداية والنهاية
        string varName = inputField.text.Trim();

        // 2. التحقق: هل الحقل فارغ؟
        if (string.IsNullOrEmpty(varName))
        {
            validationText.text = "يا بطل، اكتب اسم للصدفة أولاً!";
            return; // توقف ولا تكمل الحفظ
        }

        // 3. القيد البرمجي: منع البدء برقم
        if (char.IsDigit(varName[0]))
        {
            validationText.text = "خطأ: اسم المتغير لا يمكن أن يبدأ برقم!";
            return; // توقف
        }

        // 4. القيد البرمجي: منع وجود مسافات داخل الاسم
        if (varName.Contains(" "))
        {
            validationText.text = "خطأ: لا تستخدم المسافات في اسم المتغير.";
            return; // توقف
        }

        // 5. التحقق من تكرار الاسم (قاعدة البيانات الفريدة)
        if (savedVariables.ContainsKey(varName))
        {
            validationText.text = "هذا الاسم محجوز لصدفة أخرى، اختر اسماً جديداً.";
            return; // توقف
        }

        // 6. التأكد من عدم تجاوز عدد الأصداف المسموح به (منطقك الأصلي)
        if (container.childCount < 3)
        {
            // --- مرحلة الحفظ الفعلي (الاسم الآن سليم 100%) ---

            savedVariables.Add(varName, currentShellID);
            Debug.Log($"تم الحفظ: {varName} مرتبطة بالصدفة رقم {currentShellID}");

            // إرجاع الصدفة للأرض (تفعيلها في المشهد)
            if (beachShells != null && beachShells.Length > currentShellID)
            {
                beachShells[currentShellID].SetActive(true);
            }

            // إنشاء البلوك الصغير في القائمة الجانبية
            GameObject newVar = Instantiate(variablePrefab, container, false);

            // تحديث النص داخل البلوك الصغير
            TextMeshProUGUI textComp = newVar.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = varName;
            }

            // إذا وصلنا لـ 3 أصداف، ننشئ بلوك المتغيرات النهائي (Dropdown Block)
            if (container.childCount == 3)
            {
                CreateFinalVariableBlock();
            }

            // تصفير رسالة التنبيه وإغلاق النافذة بنجاح
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

        // استئناف الأنيمشن بعد إغلاق البوب اب
        if (girlAnimator != null)
            girlAnimator.speed = 1f;
    }

    // --- 3. نظام الصندوق السحري ---

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

        // تتحرك للصندوق في ثانية وتختفي
        shell.transform
            .DOMove(magicBox.position, 0.8f)
            .SetEase(Ease.InBack)
            .OnComplete(() => shell.SetActive(false));
            Debug.Log("MoveShellToBox called: " + shellID);

    }


    // قائمة البلوكات المرتبة في الحل
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
        float spacing = 1.5f; // المسافة بين كل صدفة
        int index = 0;

        foreach (int shellID in order)
        {
            if (shellID < 0 || shellID >= beachShells.Length) continue;

            GameObject shell = beachShells[shellID];
            if (shell == null) continue;

            shell.SetActive(true);
            shell.transform.position = magicBox.position;

            // كل صدفة تنزل في موقع مختلف على المحور X
            Vector3 targetPos = shellSpawnPoint.position + new Vector3(index * spacing, 0, 0);

            shell.transform
                .DOMove(targetPos, 0.8f)
                .SetEase(Ease.OutBack);

            index++;
            yield return new WaitForSeconds(delayBetweenShells);
        }

        // --- الإضافة هنا ---
        // ننتظر ثانية واحدة مثلاً بعد خروج آخر صدفة واستقرارها
        yield return new WaitForSeconds(1.0f);

        // إظهار بوب أب النجاح
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
            myAudioSource.PlayOneShot(celebrationSound, 1f); // 1f = أعلى صوت
    }


}








