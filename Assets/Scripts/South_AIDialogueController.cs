using UnityEngine;

public class SouthAIDialogueController : MonoBehaviour
{
    [Header("All Bubbles")]
    public GameObject bubble00_FunctionExplain;
    public GameObject bubble01_FunctionExplain;
    public GameObject bubble02_DragBlocks;
    public GameObject bubble03_StartWithDefine;
    public GameObject bubble04_AddGheeHoney;
    public GameObject bubble05_MixIngredients;
    public GameObject bubble06_ServeGuests;
    public GameObject bubble07_WriteFunctionName;
    public GameObject bubble08_RunFunction;
    public GameObject bubble09_FunctionSuccess;
    public GameObject bubble10_WrongOrder;
    public GameObject bubble11_WrongPlace;

    private void Start()
    {
        ShowBubble(bubble00_FunctionExplain);
    }

    public void HideAll()
    {
        SetBubble(bubble00_FunctionExplain, false);
        SetBubble(bubble01_FunctionExplain, false);
        SetBubble(bubble02_DragBlocks, false);
        SetBubble(bubble03_StartWithDefine, false);
        SetBubble(bubble04_AddGheeHoney, false);
        SetBubble(bubble05_MixIngredients, false);
        SetBubble(bubble06_ServeGuests, false);
        SetBubble(bubble07_WriteFunctionName, false);
        SetBubble(bubble08_RunFunction, false);
        SetBubble(bubble09_FunctionSuccess, false);
        SetBubble(bubble10_WrongOrder, false);
        SetBubble(bubble11_WrongPlace, false);
    }

    public void ShowBubble(GameObject targetBubble)
    {
        HideAll();
        SetBubble(targetBubble, true);
    }

    public void ShowIntroExplain() => ShowBubble(bubble00_FunctionExplain);
    public void ShowFunctionExplain() => ShowBubble(bubble01_FunctionExplain);
    public void ShowDragBlocks() => ShowBubble(bubble02_DragBlocks);
    public void ShowStartWithDefine() => ShowBubble(bubble03_StartWithDefine);
    public void ShowAddGheeHoney() => ShowBubble(bubble04_AddGheeHoney);
    public void ShowMixIngredients() => ShowBubble(bubble05_MixIngredients);
    public void ShowServeGuests() => ShowBubble(bubble06_ServeGuests);
    public void ShowWriteFunctionName() => ShowBubble(bubble07_WriteFunctionName);
    public void ShowRunFunction() => ShowBubble(bubble08_RunFunction);
    public void ShowFunctionSuccess() => ShowBubble(bubble09_FunctionSuccess);
    public void ShowWrongOrder() => ShowBubble(bubble10_WrongOrder);
    public void ShowWrongPlace() => ShowBubble(bubble11_WrongPlace);


    public void ShowState(string stateName)
    {
        // توجيه ذكي: يترجم الحالة النصية القادمة من البلوكات ويعرض الفقاعة المناسبة لها
        if (stateName.ToLower().Contains("success")) ShowFunctionSuccess();
        else if (stateName.ToLower().Contains("order")) ShowWrongOrder();
        else if (stateName.ToLower().Contains("place")) ShowWrongPlace();
        else ShowIntroExplain();
    }

    public void ShowErrorMessage(string message)
    {
        // توجيه افتراضي عند حدوث خطأ منطقي أثناء ترتيب الأكواد
        ShowWrongOrder();
    }

    public void ShowSuccessMessage(string message)
    {
        // توجيه تلقائي عند نجاح الطفل في حل التحدي البرمجي
        ShowFunctionSuccess();
    }

    private void SetBubble(GameObject bubble, bool state)
    {
        if (bubble != null)
            bubble.SetActive(state);
    }




}
