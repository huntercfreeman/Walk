using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.DotNetSolution;

public class DotNetSolutionCompilationUnit : ICompilationUnit
{
    public List<TextEditorTextSpan> TextSpanList { get; set; }
}
