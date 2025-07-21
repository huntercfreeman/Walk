using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Css.SyntaxEnums;

namespace Walk.CompilerServices.Css;

public interface ICssSyntax
{
    public CssSyntaxKind CssSyntaxKind { get; }
    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<ICssSyntax> ChildCssSyntaxes { get; }
}
