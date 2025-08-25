using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Json;

public struct JsonLexerOutput
{
    public JsonLexerOutput(List<TextEditorTextSpan> textSpanList)
    {
        TextSpanList = textSpanList;
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
