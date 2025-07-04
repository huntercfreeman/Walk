namespace Walk.TextEditor.RazorLib.Lexers.Models;

public record struct TextEditorTextSpan(
    int StartInclusiveIndex,
    int EndExclusiveIndex,
    byte DecorationByte)
{
    public int Length => EndExclusiveIndex - StartInclusiveIndex;
    public bool ConstructorWasInvoked => this != default;
}
