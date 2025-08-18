using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Razor;

public struct RazorLexerOutput
{
    public RazorLexerOutput()
    {
        TextSpanList = new();
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
