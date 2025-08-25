using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Razor;

public struct RazorLexerOutput
{
    public RazorLexerOutput(List<TextEditorTextSpan> textSpan)
    {
        TextSpanList = textSpan;
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
