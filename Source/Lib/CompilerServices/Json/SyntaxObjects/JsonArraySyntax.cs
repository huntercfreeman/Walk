using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Json.SyntaxEnums;

namespace Walk.CompilerServices.Json.SyntaxObjects;

public class JsonArraySyntax : IJsonSyntax
{
    public JsonArraySyntax(
        TextEditorTextSpan textEditorTextSpan,
        IReadOnlyList<JsonObjectSyntax> childJsonObjectSyntaxes)
    {
        TextEditorTextSpan = textEditorTextSpan;
        ChildJsonObjectSyntaxes = childJsonObjectSyntaxes;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<JsonObjectSyntax> ChildJsonObjectSyntaxes { get; }
    public IReadOnlyList<IJsonSyntax> ChildJsonSyntaxes => new List<IJsonSyntax>
    {

    }.Union(ChildJsonObjectSyntaxes)
        .ToList();

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.Array;
}
