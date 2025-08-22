using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.JavaScript;

public struct JavaScriptLexerOutput
{
    public JavaScriptLexerOutput()
    {
        TextSpanList = new();
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
