namespace Walk.TextEditor.RazorLib.Lexers.Models;

public record struct TextEditorTextSpan(
    int StartInclusiveIndex,
    int EndExclusiveIndex,
    byte DecorationByte)
{
    public TextEditorTextSpan(
            int startInclusiveIndex,
            int endExclusiveIndex,
            byte decorationByte,
            int byteIndex)
        : this(
            startInclusiveIndex,
            endExclusiveIndex,
            decorationByte)
    {
        ByteIndex = byteIndex;
    }
    
    public TextEditorTextSpan(
            int startInclusiveIndex,
            int endExclusiveIndex,
            byte decorationByte,
            int byteIndex,
            int charIntSum)
        : this(
            startInclusiveIndex,
            endExclusiveIndex,
            decorationByte)
    {
        ByteIndex = byteIndex;
        CharIntSum = charIntSum;
    }

    public int Length => EndExclusiveIndex - StartInclusiveIndex;
    public bool ConstructorWasInvoked => this != default;

    public int ByteIndex { get; init; }
    /// <summary>
    /// Some text spans have the text available at time of construction.
    ///
    /// Furthermore, it is negligible for them to cast each char
    /// that exists at the text span positions and sum them, then provide this data.
    /// 
    /// This value is not unique, but it has a low amount of collisions relatively speaking.
    /// Thus, you can use this value to predict whether two text spans could possibly equal eachother
    /// without having to open a StreamReader into the files and compare character by character.
    ///
    /// When you combine this check with checking the lengths of the two text spans,
    /// a vast majority of string comparisons can be avoided.
    ///
    /// One must check whether this value is != 0, prior to performing a string equality prediction with it.
    ///
    /// Because, not every text span is made while iterating the text, thus this isn't always available.
    ///
    /// !!!!
    /// It was verified that in the CSharpParserModel, all of these values
    /// are non-zero.
    ///
    /// Thus, any checks for zero have been removed from that location in particular.
    /// !!!!
    /// </summary>
    public int CharIntSum { get; init; }

    public string? GetText(string sourceText, TextEditorService? textEditorService)
    {
        if (StartInclusiveIndex < sourceText.Length && EndExclusiveIndex <= sourceText.Length && EndExclusiveIndex >= StartInclusiveIndex)
        {
            if (textEditorService is null)
            {
                return sourceText.Substring(StartInclusiveIndex, Length);
            }
            else
            {
                return textEditorService.EditContext_GetText(
                    sourceText.AsSpan(StartInclusiveIndex, Length));
            }
        }
        
        return null;
    }
}
