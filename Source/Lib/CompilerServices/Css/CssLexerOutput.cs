using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Css;

public struct CssLexerOutput
{
    public CssLexerOutput()
    {
        TextSpanList = new();
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
