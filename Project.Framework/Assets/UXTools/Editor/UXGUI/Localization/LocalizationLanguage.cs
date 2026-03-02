using UnityEngine;
using ThunderFireUITool;

public class LocalizationLanguage
{
    public const int ChineseSimplifiedIndex = 0;

    private static readonly long[] m_languages = new long[] {
        EditorLocalizationStorage.Def_简体中文,
    };

    public static int Length
    {
        get
        {
            return m_languages.Length;
        }
    }
    public static string GetLanguage(int index)
    {
        if (index == ChineseSimplifiedIndex)
        {
            return "简体中文  (CN)";
        }

        return "简体中文";
    }
}
