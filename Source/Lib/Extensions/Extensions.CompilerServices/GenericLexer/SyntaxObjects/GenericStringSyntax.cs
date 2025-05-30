using Walk.Extensions.CompilerServices.GenericLexer.SyntaxEnums;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.GenericLexer.SyntaxObjects;

public class GenericStringSyntax : IGenericSyntax
{
	public GenericStringSyntax(TextEditorTextSpan textSpan)
	{
		TextSpan = textSpan;
	}

	public TextEditorTextSpan TextSpan { get; }
	public IReadOnlyList<IGenericSyntax> ChildList => Array.Empty<IGenericSyntax>();
	public GenericSyntaxKind GenericSyntaxKind => GenericSyntaxKind.StringLiteral;
}