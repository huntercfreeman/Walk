using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Xml;

public class XmlCompilationUnit : ICompilationUnit
{
    public List<TextEditorTextSpan> TextSpanList { get; set; }
}
