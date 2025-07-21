namespace Walk.TextEditor.RazorLib.Lexers.Models;

public record struct TextEditorTextSpan(
    int StartInclusiveIndex,
    int EndExclusiveIndex,
    byte DecorationByte)
{
    public int Length => EndExclusiveIndex - StartInclusiveIndex;
    public bool ConstructorWasInvoked => this != default;

    public string? GetText(string sourceText, TextEditorService? textEditorService)
    {
        if (StartInclusiveIndex < sourceText.Length && EndExclusiveIndex <= sourceText.Length && EndExclusiveIndex >= StartInclusiveIndex)
        {
            if (textEditorService is null)
            {
                Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringAllocation++;
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
