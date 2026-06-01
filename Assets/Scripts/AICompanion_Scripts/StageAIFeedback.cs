using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[System.Serializable]
public class StageAIFeedback
{
    public AICompanionController aiCompanion;
    public string storyKey;
    public string defaultGender = "male";

    private bool initialized;
    private readonly HashSet<string> playedSuccessSignatures = new HashSet<string>();

    public AICompanionController EnsureInitialized(MonoBehaviour owner, string fallbackStoryKey)
    {
        if (aiCompanion == null && AICompanionController.Instance != null)
            aiCompanion = AICompanionController.Instance;

        if (aiCompanion == null)
            aiCompanion = Object.FindAnyObjectByType<AICompanionController>();

        if (aiCompanion == null && owner != null)
            aiCompanion = owner.gameObject.AddComponent<AICompanionController>();

        if (aiCompanion == null)
            return null;

        if (string.IsNullOrWhiteSpace(storyKey))
            storyKey = fallbackStoryKey;

        if (!initialized)
        {
            aiCompanion.InitializeCompanion(storyKey, ResolveGender());
            initialized = true;
        }

        return aiCompanion;
    }

    public bool RequestWrong(
        MonoBehaviour owner,
        string fallbackStoryKey,
        string signature,
        string instruction)
    {
        AICompanionController companion = EnsureInitialized(owner, fallbackStoryKey);

        ShowTextBoxFeedbackIfSupported(owner, fallbackStoryKey, instruction);

        if (companion == null)
            return false;

        AICompanionController.CancelAllPendingVoiceFeedback();

        bool started = companion.RequestExactVoiceFeedback(
            "",
            instruction,
            false,
            false
        );

        return started;
    }

    public bool RequestSuccess(
        MonoBehaviour owner,
        string fallbackStoryKey,
        string signature,
        string instruction)
    {
        string successSignature = string.IsNullOrWhiteSpace(signature)
            ? fallbackStoryKey + "_success"
            : signature;

        if (!playedSuccessSignatures.Add(successSignature))
            return false;

        AICompanionController companion = EnsureInitialized(owner, fallbackStoryKey);
        if (companion == null)
            return false;

        companion.CancelPendingVoiceFeedback();
        AICompanionController.CancelAllPendingVoiceFeedback();
        ShowTextBoxFeedbackIfSupported(owner, fallbackStoryKey, instruction);

        bool started = companion.RequestExactVoiceFeedback(
            successSignature,
            instruction,
            true,
            true
        );

        return started;
    }

    public void ClearWrongRepeat()
    {
    }

    public int GetAttemptCount()
    {
        return aiCompanion != null ? aiCompanion.GetAttemptCount() : 0;
    }

    private string ResolveGender()
    {
        return PlayerVoiceResolver.GetStoredGender(defaultGender);
    }

    private void ShowTextBoxFeedbackIfSupported(MonoBehaviour owner, string fallbackStoryKey, string message)
    {
        if (owner == null || string.IsNullOrWhiteSpace(message))
            return;

        string key = string.IsNullOrWhiteSpace(storyKey) ? fallbackStoryKey : storyKey;
        string targetName = GetFeedbackTextObjectName(key);
        if (string.IsNullOrWhiteSpace(targetName))
            return;

        TextMeshProUGUI targetText = FindFeedbackText(owner, targetName, true);
        if (targetText == null)
        {
            Debug.LogWarning("[AI Text Box] لم يتم العثور على " + targetName + " في مشهد: " + owner.gameObject.scene.name);
            return;
        }

        ActivateHierarchyToTarget(targetText.transform, targetName);
        targetText.gameObject.SetActive(true);
        ArabicTextFormatter.ApplyTo(targetText, message);

        Debug.Log("[AI Text Box] " + message);
    }

    private string GetFeedbackTextObjectName(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return "";

        key = key.Trim().ToLowerInvariant();
        switch (key)
        {
            case "north":
            case "east":
            case "qassim":
                return "text_box";

            case "riyadh":
                return "AssistantArea";

            case "south":
                return "AI_ChatSystem";

            default:
                return "";
        }
    }

    private TextMeshProUGUI FindFeedbackText(MonoBehaviour owner, string targetName, bool createIfMissing)
    {
        GameObject[] roots = owner.gameObject.scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform textRoot = FindChildRecursive(root.transform, targetName);
            if (textRoot == null)
                continue;

            TextMeshProUGUI text = textRoot.GetComponent<TextMeshProUGUI>();
            if (text != null)
                return text;

            text = textRoot.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
                return text;

            if (createIfMissing)
            {
                text = CreateFeedbackText(textRoot);
                if (text != null)
                    return text;
            }
        }

        return null;
    }

    private TextMeshProUGUI CreateFeedbackText(Transform textRoot)
    {
        if (textRoot == null)
            return null;

        Transform parent = textRoot;
        RectTransform parentRect = textRoot as RectTransform;

        if (parentRect == null)
        {
            Canvas childCanvas = textRoot.GetComponentInChildren<Canvas>(true);
            if (childCanvas != null)
            {
                parent = childCanvas.transform;
                parentRect = parent as RectTransform;
            }
        }

        if (parentRect == null)
            return null;

        GameObject textObject = new GameObject(
            "AI Feedback Text - RTLTMP",
            typeof(RectTransform),
            typeof(CanvasRenderer)
        );

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(parent, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 8f);
        textRect.offsetMax = new Vector2(-12f, -8f);
        textRect.pivot = new Vector2(0.5f, 0.5f);

        System.Type rtlTextType = FindRtlTextMeshProType();
        TextMeshProUGUI text = rtlTextType != null
            ? textObject.AddComponent(rtlTextType) as TextMeshProUGUI
            : textObject.AddComponent<TextMeshProUGUI>();

        if (text == null)
            return null;

        text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.Right;
        text.isRightToLeftText = true;
        text.color = Color.black;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Truncate;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12f;
        text.fontSizeMax = 22f;

        TMP_FontAsset arabicFont = ArabicTextFormatter.FindBestArabicFont(textRoot);
        if (arabicFont != null)
            text.font = arabicFont;

        return text;
    }

    private System.Type FindRtlTextMeshProType()
    {
        return FindTypeByName("RTLTMPro.RTLTextMeshPro");
    }

    private System.Type FindTypeByName(string typeName)
    {
        foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            System.Type type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }

    private TMP_FontAsset FindArabicFontInScene(Transform textRoot)
    {
        if (textRoot == null || !textRoot.gameObject.scene.IsValid())
            return null;

        TextMeshProUGUI[] sceneTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in sceneTexts)
        {
            if (text == null || text.font == null)
                continue;

            if (!text.gameObject.scene.IsValid() ||
                text.gameObject.scene != textRoot.gameObject.scene)
                continue;

            string fontName = text.font.name.ToLowerInvariant();
            if (fontName.Contains("arabic") ||
                fontName.Contains("tufuli") ||
                fontName.Contains("baloo"))
            {
                return text.font;
            }
        }

        TMP_FontAsset[] loadedFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in loadedFonts)
        {
            if (font == null)
                continue;

            string fontName = font.name.ToLowerInvariant();
            if (fontName.Contains("arabic") ||
                fontName.Contains("tufuli") ||
                fontName.Contains("baloo"))
            {
                return font;
            }
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent.name == targetName)
            return parent;

        foreach (Transform child in parent)
        {
            Transform found = FindChildRecursive(child, targetName);
            if (found != null)
                return found;
        }

        return null;
    }

    private void ActivateHierarchyToTarget(Transform child, string targetName)
    {
        Transform current = child;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
                current.gameObject.SetActive(true);

            if (current.name == targetName)
                break;

            current = current.parent;
        }
    }
}

public static class ArabicTextFormatter
{
    private struct ArabicForms
    {
        public readonly char Isolated;
        public readonly char Final;
        public readonly char Initial;
        public readonly char Medial;

        public ArabicForms(int isolated, int final, int initial, int medial)
        {
            Isolated = (char)isolated;
            Final = (char)final;
            Initial = (char)initial;
            Medial = (char)medial;
        }

        public bool HasFinal => Final != '\0';
        public bool HasInitial => Initial != '\0';
        public bool HasMedial => Medial != '\0';
    }

    private static readonly Dictionary<char, ArabicForms> Forms =
        new Dictionary<char, ArabicForms>
        {
            { (char)0x0621, new ArabicForms(0xFE80, 0, 0, 0) },
            { (char)0x0622, new ArabicForms(0xFE81, 0xFE82, 0, 0) },
            { (char)0x0623, new ArabicForms(0xFE83, 0xFE84, 0, 0) },
            { (char)0x0624, new ArabicForms(0xFE85, 0xFE86, 0, 0) },
            { (char)0x0625, new ArabicForms(0xFE87, 0xFE88, 0, 0) },
            { (char)0x0626, new ArabicForms(0xFE89, 0xFE8A, 0xFE8B, 0xFE8C) },
            { (char)0x0627, new ArabicForms(0xFE8D, 0xFE8E, 0, 0) },
            { (char)0x0628, new ArabicForms(0xFE8F, 0xFE90, 0xFE91, 0xFE92) },
            { (char)0x0629, new ArabicForms(0xFE93, 0xFE94, 0, 0) },
            { (char)0x062A, new ArabicForms(0xFE95, 0xFE96, 0xFE97, 0xFE98) },
            { (char)0x062B, new ArabicForms(0xFE99, 0xFE9A, 0xFE9B, 0xFE9C) },
            { (char)0x062C, new ArabicForms(0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0) },
            { (char)0x062D, new ArabicForms(0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4) },
            { (char)0x062E, new ArabicForms(0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8) },
            { (char)0x062F, new ArabicForms(0xFEA9, 0xFEAA, 0, 0) },
            { (char)0x0630, new ArabicForms(0xFEAB, 0xFEAC, 0, 0) },
            { (char)0x0631, new ArabicForms(0xFEAD, 0xFEAE, 0, 0) },
            { (char)0x0632, new ArabicForms(0xFEAF, 0xFEB0, 0, 0) },
            { (char)0x0633, new ArabicForms(0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4) },
            { (char)0x0634, new ArabicForms(0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8) },
            { (char)0x0635, new ArabicForms(0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC) },
            { (char)0x0636, new ArabicForms(0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0) },
            { (char)0x0637, new ArabicForms(0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4) },
            { (char)0x0638, new ArabicForms(0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8) },
            { (char)0x0639, new ArabicForms(0xFEC9, 0xFECA, 0xFECB, 0xFECC) },
            { (char)0x063A, new ArabicForms(0xFECD, 0xFECE, 0xFECF, 0xFED0) },
            { (char)0x0641, new ArabicForms(0xFED1, 0xFED2, 0xFED3, 0xFED4) },
            { (char)0x0642, new ArabicForms(0xFED5, 0xFED6, 0xFED7, 0xFED8) },
            { (char)0x0643, new ArabicForms(0xFED9, 0xFEDA, 0xFEDB, 0xFEDC) },
            { (char)0x0644, new ArabicForms(0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0) },
            { (char)0x0645, new ArabicForms(0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4) },
            { (char)0x0646, new ArabicForms(0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8) },
            { (char)0x0647, new ArabicForms(0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC) },
            { (char)0x0648, new ArabicForms(0xFEED, 0xFEEE, 0, 0) },
            { (char)0x0649, new ArabicForms(0xFEEF, 0xFEF0, 0, 0) },
            { (char)0x064A, new ArabicForms(0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4) },
        };

    public static void ApplyTo(TextMeshProUGUI text, string value)
    {
        if (text == null)
            return;

        TMP_FontAsset font = FindBestArabicFont(text.transform);
        if (font != null && !SupportsRequiredArabicForms(text.font))
            text.font = font;

        text.isRightToLeftText = true;
        text.alignment = TextAlignmentOptions.Right;
        text.text = Format(value, text.font);
        text.ForceMeshUpdate();
    }

    public static TMP_FontAsset FindBestArabicFont(Transform sceneContext)
    {
        TMP_FontAsset sceneFont = FindSceneFontWithPresentationForms(sceneContext);
        if (sceneFont != null)
            return sceneFont;

        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in fonts)
        {
            if (SupportsRequiredArabicForms(font) && IsPreferredArabicFont(font))
                return font;
        }

        foreach (TMP_FontAsset font in fonts)
        {
            if (SupportsRequiredArabicForms(font))
                return font;
        }

        foreach (TMP_FontAsset font in fonts)
        {
            if (SupportsPresentationForms(font) && IsPreferredArabicFont(font))
                return font;
        }

        foreach (TMP_FontAsset font in fonts)
        {
            if (SupportsPresentationForms(font))
                return font;
        }

        return null;
    }

    public static string Format(string value)
    {
        return Format(value, null);
    }

    public static string Format(string value, TMP_FontAsset font)
    {
        if (string.IsNullOrWhiteSpace(value) || !ContainsArabic(value))
            return value ?? "";

        return Shape(value, font);
    }

    private static string Shape(string value, TMP_FontAsset font)
    {
        StringBuilder shaped = new StringBuilder(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];
            if (!Forms.TryGetValue(current, out ArabicForms currentForms))
            {
                shaped.Append(current);
                continue;
            }

            char previous = FindPreviousArabicLetter(value, i);
            char next = FindNextArabicLetter(value, i);
            bool connectsPrevious = CanConnectAfter(previous) && CanConnectBefore(current);
            bool connectsNext = CanConnectAfter(current) && CanConnectBefore(next);

            char shapedCharacter;
            if (connectsPrevious && connectsNext && currentForms.HasMedial)
                shapedCharacter = currentForms.Medial;
            else if (connectsPrevious && currentForms.HasFinal)
                shapedCharacter = currentForms.Final;
            else if (connectsNext && currentForms.HasInitial)
                shapedCharacter = currentForms.Initial;
            else
                shapedCharacter = currentForms.Isolated;

            shaped.Append(GetSupportedCharacter(font, current, shapedCharacter));
        }

        return shaped.ToString();
    }

    private static char GetSupportedCharacter(TMP_FontAsset font, char original, char shaped)
    {
        if (font == null)
            return shaped;

        if (font.HasCharacter(shaped))
            return shaped;

        if (font.HasCharacter(original))
            return original;

        return shaped;
    }

    private static string ReverseForTextMeshPro(string value)
    {
        StringBuilder reversed = new StringBuilder(value.Length);

        for (int i = value.Length - 1; i >= 0; i--)
        {
            if (IsLeftToRightTokenChar(value[i]))
            {
                int end = i;
                while (i >= 0 && IsLeftToRightTokenChar(value[i]))
                    i--;

                reversed.Append(value, i + 1, end - i);
                i++;
            }
            else
            {
                reversed.Append(value[i]);
            }
        }

        return reversed.ToString();
    }

    private static char FindPreviousArabicLetter(string value, int index)
    {
        for (int i = index - 1; i >= 0; i--)
        {
            if (IsArabicDiacritic(value[i]))
                continue;

            return Forms.ContainsKey(value[i]) ? value[i] : '\0';
        }

        return '\0';
    }

    private static char FindNextArabicLetter(string value, int index)
    {
        for (int i = index + 1; i < value.Length; i++)
        {
            if (IsArabicDiacritic(value[i]))
                continue;

            return Forms.ContainsKey(value[i]) ? value[i] : '\0';
        }

        return '\0';
    }

    private static bool CanConnectBefore(char value)
    {
        return Forms.TryGetValue(value, out ArabicForms forms) && forms.HasFinal;
    }

    private static bool CanConnectAfter(char value)
    {
        return Forms.TryGetValue(value, out ArabicForms forms) && forms.HasInitial;
    }

    private static bool ContainsArabic(string value)
    {
        foreach (char character in value)
        {
            if (Forms.ContainsKey(character))
                return true;
        }

        return false;
    }

    private static bool IsArabicDiacritic(char value)
    {
        return value >= (char)0x064B && value <= (char)0x065F;
    }

    private static bool IsLeftToRightTokenChar(char value)
    {
        return (value >= 'A' && value <= 'Z') ||
               (value >= 'a' && value <= 'z') ||
               (value >= '0' && value <= '9') ||
               value == '_' ||
               value == '-' ||
               value == '.';
    }

    private static TMP_FontAsset FindSceneFontWithPresentationForms(Transform sceneContext)
    {
        if (sceneContext == null || !sceneContext.gameObject.scene.IsValid())
            return null;

        TextMeshProUGUI[] texts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null || text.font == null)
                continue;

            if (!text.gameObject.scene.IsValid() ||
                text.gameObject.scene != sceneContext.gameObject.scene)
                continue;

            if (SupportsRequiredArabicForms(text.font) && IsPreferredArabicFont(text.font))
                return text.font;
        }

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null || text.font == null)
                continue;

            if (!text.gameObject.scene.IsValid() ||
                text.gameObject.scene != sceneContext.gameObject.scene)
                continue;

            if (SupportsRequiredArabicForms(text.font))
                return text.font;
        }

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null || text.font == null)
                continue;

            if (!text.gameObject.scene.IsValid() ||
                text.gameObject.scene != sceneContext.gameObject.scene)
                continue;

            if (SupportsPresentationForms(text.font) && IsPreferredArabicFont(text.font))
                return text.font;
        }

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null || text.font == null)
                continue;

            if (!text.gameObject.scene.IsValid() ||
                text.gameObject.scene != sceneContext.gameObject.scene)
                continue;

            if (SupportsPresentationForms(text.font))
                return text.font;
        }

        return null;
    }

    private static bool SupportsPresentationForms(TMP_FontAsset font)
    {
        return font != null &&
               font.HasCharacter((char)0xFE91) &&
               font.HasCharacter((char)0xFEE0);
    }

    private static bool SupportsRequiredArabicForms(TMP_FontAsset font)
    {
        return SupportsPresentationForms(font) &&
               font.HasCharacter((char)0xFE80) &&
               font.HasCharacter((char)0xFE83) &&
               font.HasCharacter((char)0xFE84) &&
               font.HasCharacter((char)0xFE87) &&
               font.HasCharacter((char)0xFE88) &&
               font.HasCharacter((char)0xFE8D) &&
               font.HasCharacter((char)0xFE8E);
    }

    private static bool IsPreferredArabicFont(TMP_FontAsset font)
    {
        if (font == null)
            return false;

        string name = font.name.ToLowerInvariant();
        return name.Contains("tufuli") ||
               name.Contains("arabic") ||
               name.Contains("cairo") ||
               name.Contains("baloo");
    }
}
