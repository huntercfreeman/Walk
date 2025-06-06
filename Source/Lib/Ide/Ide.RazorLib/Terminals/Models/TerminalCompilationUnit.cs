using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalCompilationUnit : ICompilationUnit
{
	public IReadOnlyList<SyntaxToken> SyntaxTokenList { get; set; } = new List<SyntaxToken>();
    public List<TextEditorTextSpan> ManualDecorationTextSpanList { get; } = new List<TextEditorTextSpan>();
    public List<Symbol> ManualSymbolList { get; } = new List<Symbol>();

	public IEnumerable<TextEditorTextSpan> GetTextTextSpans()
	{
		return SyntaxTokenList.Select(x => x.TextSpan)
			.Concat(ManualDecorationTextSpanList)
			.Concat(ManualSymbolList.Select(x => x.TextSpan));
	}
	
	public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans()
	{
		return Array.Empty<TextEditorTextSpan>();
	}
}
