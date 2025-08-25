using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Css;

public struct CssLexerOutput
{
    public CssLexerOutput(List<TextEditorTextSpan> textSpanList)
    {
        TextSpanList = textSpanList;
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
