using Walk.Common.RazorLib;

namespace Walk.TextEditor.RazorLib.Characters.Models;

public static class CharacterKindHelper
{
    public static CharacterKind CharToCharacterKind(char value)
    {
        if (CommonFacts.IsWhitespaceCharacter(value))
            return CharacterKind.Whitespace;
        if (CommonFacts.IsPunctuationCharacter(value))
            return CharacterKind.Punctuation;
        if (value == '\0')
            return CharacterKind.Bad;
        return CharacterKind.LetterOrDigit;
    }
    
    public static int GetPriorityGolfRank(CharacterKind characterKind)
    {
        if (characterKind == CharacterKind.LetterOrDigit)
        {
            return 0;
        }
        else if (characterKind == CharacterKind.Punctuation)
        {
            return 1;
        }
        else if (characterKind == CharacterKind.Whitespace)
        {
            return 2;
        }
        else if (characterKind == CharacterKind.Bad)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }
}
