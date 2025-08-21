using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Css;

public class CssCompilationUnit : ICompilationUnit
{
    public List<TextEditorTextSpan> TextSpanList { get; set; }
}

