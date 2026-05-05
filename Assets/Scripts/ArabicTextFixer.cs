using System.Collections.Generic;
using System.Text;

public static class ArabicTextFixer
{
    private class ArabicForm
    {
        public char Isolated;
        public char Final;
        public char Initial;
        public char Medial;
        public bool ConnectsToNext;

        public ArabicForm(char isolated, char final, char initial, char medial, bool connectsToNext)
        {
            Isolated = isolated;
            Final = final;
            Initial = initial;
            Medial = medial;
            ConnectsToNext = connectsToNext;
        }
    }

    private static readonly Dictionary<char, ArabicForm> Forms = new Dictionary<char, ArabicForm>()
    {
        ['ا'] = new ArabicForm('ﺍ', 'ﺎ', '\0', '\0', false),
        ['أ'] = new ArabicForm('ﺃ', 'ﺄ', '\0', '\0', false),
        ['إ'] = new ArabicForm('ﺇ', 'ﺈ', '\0', '\0', false),
        ['آ'] = new ArabicForm('ﺁ', 'ﺂ', '\0', '\0', false),
        ['ب'] = new ArabicForm('ﺏ', 'ﺐ', 'ﺑ', 'ﺒ', true),
        ['ت'] = new ArabicForm('ﺕ', 'ﺖ', 'ﺗ', 'ﺘ', true),
        ['ث'] = new ArabicForm('ﺙ', 'ﺚ', 'ﺛ', 'ﺜ', true),
        ['ج'] = new ArabicForm('ﺝ', 'ﺞ', 'ﺟ', 'ﺠ', true),
        ['ح'] = new ArabicForm('ﺡ', 'ﺢ', 'ﺣ', 'ﺤ', true),
        ['خ'] = new ArabicForm('ﺥ', 'ﺦ', 'ﺧ', 'ﺨ', true),
        ['د'] = new ArabicForm('ﺩ', 'ﺪ', '\0', '\0', false),
        ['ذ'] = new ArabicForm('ﺫ', 'ﺬ', '\0', '\0', false),
        ['ر'] = new ArabicForm('ﺭ', 'ﺮ', '\0', '\0', false),
        ['ز'] = new ArabicForm('ﺯ', 'ﺰ', '\0', '\0', false),
        ['س'] = new ArabicForm('ﺱ', 'ﺲ', 'ﺳ', 'ﺴ', true),
        ['ش'] = new ArabicForm('ﺵ', 'ﺶ', 'ﺷ', 'ﺸ', true),
        ['ص'] = new ArabicForm('ﺹ', 'ﺺ', 'ﺻ', 'ﺼ', true),
        ['ض'] = new ArabicForm('ﺽ', 'ﺾ', 'ﺿ', 'ﻀ', true),
        ['ط'] = new ArabicForm('ﻁ', 'ﻂ', 'ﻃ', 'ﻄ', true),
        ['ظ'] = new ArabicForm('ﻅ', 'ﻆ', 'ﻇ', 'ﻈ', true),
        ['ع'] = new ArabicForm('ﻉ', 'ﻊ', 'ﻋ', 'ﻌ', true),
        ['غ'] = new ArabicForm('ﻍ', 'ﻎ', 'ﻏ', 'ﻐ', true),
        ['ف'] = new ArabicForm('ﻑ', 'ﻒ', 'ﻓ', 'ﻔ', true),
        ['ق'] = new ArabicForm('ﻕ', 'ﻖ', 'ﻗ', 'ﻘ', true),
        ['ك'] = new ArabicForm('ﻙ', 'ﻚ', 'ﻛ', 'ﻜ', true),
        ['ل'] = new ArabicForm('ﻝ', 'ﻞ', 'ﻟ', 'ﻠ', true),
        ['م'] = new ArabicForm('ﻡ', 'ﻢ', 'ﻣ', 'ﻤ', true),
        ['ن'] = new ArabicForm('ﻥ', 'ﻦ', 'ﻧ', 'ﻨ', true),
        ['ه'] = new ArabicForm('ﻩ', 'ﻪ', 'ﻫ', 'ﻬ', true),
        ['و'] = new ArabicForm('ﻭ', 'ﻮ', '\0', '\0', false),
        ['ؤ'] = new ArabicForm('ﺅ', 'ﺆ', '\0', '\0', false),
        ['ى'] = new ArabicForm('ﻯ', 'ﻰ', '\0', '\0', false),
        ['ي'] = new ArabicForm('ﻱ', 'ﻲ', 'ﻳ', 'ﻴ', true),
        ['ئ'] = new ArabicForm('ﺉ', 'ﺊ', 'ﺋ', 'ﺌ', true),
        ['ة'] = new ArabicForm('ﺓ', 'ﺔ', '\0', '\0', false),
        ['ء'] = new ArabicForm('ء', 'ء', '\0', '\0', false),
    };

    public static string Fix(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string[] lines = input.Split('\n');
        StringBuilder finalBuilder = new StringBuilder();

        for (int l = 0; l < lines.Length; l++)
        {
            string shaped = ShapeLine(lines[l]);
            char[] arr = shaped.ToCharArray();
            System.Array.Reverse(arr);
            finalBuilder.Append(new string(arr));

            if (l < lines.Length - 1)
                finalBuilder.Append('\n');
        }

        return finalBuilder.ToString();
    }

    private static string ShapeLine(string line)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char current = line[i];

            if (!Forms.ContainsKey(current))
            {
                sb.Append(current);
                continue;
            }

            char prev = GetPreviousArabicChar(line, i);
            char next = GetNextArabicChar(line, i);

            bool connectsPrev = CanConnect(prev, current, previousToCurrent: true);
            bool connectsNext = CanConnect(current, next, previousToCurrent: false);

            ArabicForm form = Forms[current];

            if (connectsPrev && connectsNext && form.Medial != '\0')
                sb.Append(form.Medial);
            else if (connectsPrev && form.Final != '\0')
                sb.Append(form.Final);
            else if (connectsNext && form.Initial != '\0')
                sb.Append(form.Initial);
            else
                sb.Append(form.Isolated);
        }

        return sb.ToString();
    }

    private static char GetPreviousArabicChar(string line, int index)
    {
        for (int i = index - 1; i >= 0; i--)
        {
            if (line[i] != ' ')
                return line[i];
        }
        return '\0';
    }

    private static char GetNextArabicChar(string line, int index)
    {
        for (int i = index + 1; i < line.Length; i++)
        {
            if (line[i] != ' ')
                return line[i];
        }
        return '\0';
    }

    private static bool CanConnect(char current, char other, bool previousToCurrent)
    {
        if (current == '\0' || other == '\0')
            return false;

        if (!Forms.ContainsKey(current) || !Forms.ContainsKey(other))
            return false;

        if (previousToCurrent)
        {
            return Forms[current].Final != '\0' && Forms[other].ConnectsToNext;
        }

        return Forms[current].ConnectsToNext && Forms[other].Final != '\0';
    }
}