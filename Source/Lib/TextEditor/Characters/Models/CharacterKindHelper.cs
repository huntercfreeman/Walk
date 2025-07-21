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
}
