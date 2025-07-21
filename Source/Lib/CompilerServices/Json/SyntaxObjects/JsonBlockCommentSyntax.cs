using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Json.SyntaxEnums;

namespace Walk.CompilerServices.Json.SyntaxObjects;

/// <summary>
/// Comments are not valid in Standard JSON.
/// </summary>
public class JsonBlockCommentSyntax : IJsonSyntax
{
    public JsonBlockCommentSyntax(
        TextEditorTextSpan textEditorTextSpan,
        IReadOnlyList<IJsonSyntax> childJsonSyntaxes)
    {
        ChildJsonSyntaxes = childJsonSyntaxes;
        TextEditorTextSpan = textEditorTextSpan;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<IJsonSyntax> ChildJsonSyntaxes { get; }

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.BlockComment;
}
