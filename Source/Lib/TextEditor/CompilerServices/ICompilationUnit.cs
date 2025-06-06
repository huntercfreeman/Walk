using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.CompilerServices;

public interface ICompilationUnit
{
	public IEnumerable<TextEditorTextSpan> GetTextTextSpans();
	public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans();
}
