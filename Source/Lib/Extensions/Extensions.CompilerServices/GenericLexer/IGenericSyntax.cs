using Walk.Extensions.CompilerServices.GenericLexer.SyntaxEnums;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.GenericLexer;

public interface IGenericSyntax
{
    public TextEditorTextSpan TextSpan { get; }
    public IReadOnlyList<IGenericSyntax> ChildList { get; }
    public GenericSyntaxKind GenericSyntaxKind { get; }
}
