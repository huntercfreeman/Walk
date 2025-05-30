using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Css.SyntaxEnums;

namespace Walk.CompilerServices.Css.SyntaxObjects;

public class CssCommentSyntax : ICssSyntax
{
    public CssCommentSyntax(
        TextEditorTextSpan textEditorTextSpan,
        IReadOnlyList<ICssSyntax> childCssSyntaxes)
    {
        ChildCssSyntaxes = childCssSyntaxes;
        TextEditorTextSpan = textEditorTextSpan;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<ICssSyntax> ChildCssSyntaxes { get; }

    public CssSyntaxKind CssSyntaxKind => CssSyntaxKind.Comment;
}