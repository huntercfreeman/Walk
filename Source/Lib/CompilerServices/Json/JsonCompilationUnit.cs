using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Json;

public class JsonCompilationUnit : ICompilationUnit
{
    public List<TextEditorTextSpan> TextSpanList { get; set; }
}
