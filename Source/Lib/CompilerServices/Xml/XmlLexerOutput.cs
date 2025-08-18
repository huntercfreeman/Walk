using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Xml;

public struct XmlLexerOutput
{
    public XmlLexerOutput()
    {
        TextSpanList = new();
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
}
