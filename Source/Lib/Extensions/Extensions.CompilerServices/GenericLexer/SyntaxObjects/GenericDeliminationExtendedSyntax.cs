using Walk.Extensions.CompilerServices.GenericLexer.SyntaxEnums;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.GenericLexer.SyntaxObjects;

public class GenericDeliminationExtendedSyntax : IGenericSyntax
{
    public GenericDeliminationExtendedSyntax(TextEditorTextSpan textSpan)
    {
        TextSpan = textSpan;
    }

    public TextEditorTextSpan TextSpan { get; }
    public IReadOnlyList<IGenericSyntax> ChildList => Array.Empty<IGenericSyntax>();
    public GenericSyntaxKind GenericSyntaxKind => GenericSyntaxKind.DeliminationExtended;
}
