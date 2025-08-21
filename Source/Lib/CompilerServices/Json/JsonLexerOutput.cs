using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Json;

public struct JsonLexerOutput
{
    public JsonLexerOutput()
    {
        TextSpanList = new();
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
