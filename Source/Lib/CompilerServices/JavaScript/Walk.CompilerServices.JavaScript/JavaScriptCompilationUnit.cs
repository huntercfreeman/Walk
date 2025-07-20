using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.JavaScript;

public class JavaScriptCompilationUnit : ICompilationUnit
{
	public IReadOnlyList<SyntaxToken> TokenList { get; init; } = Array.Empty<SyntaxToken>();
	public IReadOnlyList<TextEditorDiagnostic> DiagnosticList { get; init; } = Array.Empty<TextEditorDiagnostic>();

	public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans()
	{
		return DiagnosticList.Select(x => x.TextSpan);
	}
}
