using Walk.Extensions.CompilerServices.GenericLexer.SyntaxObjects;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.CompilerServices.GenericLexer;

public class GenericSyntaxUnit
{
	public GenericSyntaxUnit(
		GenericDocumentSyntax genericDocumentSyntax,
		List<TextEditorDiagnostic> diagnosticList)
	{
		GenericDocumentSyntax = genericDocumentSyntax;
		DiagnosticList = diagnosticList;
	}

	public GenericDocumentSyntax GenericDocumentSyntax { get; }
	public IReadOnlyList<TextEditorDiagnostic> DiagnosticList { get; }
}