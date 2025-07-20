using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.Razor.CompilerServiceCase;

public class RazorCompilationUnit : ICompilationUnit
{
	public IReadOnlyList<SyntaxToken> TokenList { get; init; } = Array.Empty<SyntaxToken>();
	public RazorResource RazorResource { get; init; }
	public IReadOnlyList<TextEditorDiagnostic> DiagnosticList { get; init; } = Array.Empty<TextEditorDiagnostic>();

	public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans()
	{
		return DiagnosticList.Select(x => x.TextSpan);
	}
}
