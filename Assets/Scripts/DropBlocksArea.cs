using UnityEngine;

public class DropBlocksArea : MonoBehaviour
{
    public static DropBlocksArea Instance;

    [Header("AI Dialogue")]
    public AIDialogueController aiDialogue;

    private void Awake()
    {
        Instance = this;
    }

    public void AcceptBlock(GameObject block, string blockID)
    {
        block.transform.SetParent(transform, false);

        RectTransform rect = block.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchoredPosition = Vector2.zero;

        CanvasGroup cg = block.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = block.AddComponent<CanvasGroup>();

        cg.blocksRaycasts = true;

        if (GameUISoundManager.Instance != null)
            GameUISoundManager.Instance.PlayBlock();

        BlockPopEffect pop = block.GetComponent<BlockPopEffect>();
        if (pop == null)
            pop = block.AddComponent<BlockPopEffect>();

        pop.PlayPop();

        UpdateAIBubble(blockID);
    }

    private void UpdateAIBubble(string blockID)
    {
        if (aiDialogue == null)
            return;

        switch (blockID)
        {
            case "define":
                aiDialogue.ShowStartWithDefine();
                break;

            case "flour":
                aiDialogue.ShowAddGheeHoney();
                break;

            case "ghee":
            case "honey":
                aiDialogue.ShowMixIngredients();
                break;

            case "mix":
                aiDialogue.ShowServeGuests();
                break;

            case "serve":
            case "call":
                aiDialogue.ShowDragBlocks();
                break;
        }
    }
}