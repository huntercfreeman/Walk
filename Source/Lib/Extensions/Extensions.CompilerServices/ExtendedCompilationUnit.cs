using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Extensions.CompilerServices;

public class ExtendedCompilationUnit : ICompilationUnit
{
    public IReadOnlyList<SyntaxToken> TokenList { get; init; } = Array.Empty<SyntaxToken>();
    public IReadOnlyList<TextEditorDiagnostic> DiagnosticList { get; init; } = Array.Empty<TextEditorDiagnostic>();

    public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans()
    {
        return DiagnosticList.Select(x => x.TextSpan);
    }
}
