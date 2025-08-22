using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.JavaScript;

public class JavaScriptCompilationUnit : ICompilationUnit
{
    public List<TextEditorTextSpan> TextSpanList { get; set; }
}
