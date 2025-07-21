using Walk.Extensions.CompilerServices.GenericLexer.SyntaxEnums;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.GenericLexer.SyntaxObjects;

public class GenericDocumentSyntax : IGenericSyntax
{
	public GenericDocumentSyntax(
		TextEditorTextSpan textSpan,
		IReadOnlyList<IGenericSyntax> childList)
	{
		TextSpan = textSpan;
		ChildList = childList;
	}

	public TextEditorTextSpan TextSpan { get; }
	public IReadOnlyList<IGenericSyntax> ChildList { get; }
	public GenericSyntaxKind GenericSyntaxKind => GenericSyntaxKind.Document;
}
