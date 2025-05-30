using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Json.SyntaxEnums;

namespace Walk.CompilerServices.Json.SyntaxObjects;

public class JsonObjectSyntax : IJsonSyntax
{
    public JsonObjectSyntax(
        TextEditorTextSpan textEditorTextSpan,
        IReadOnlyList<JsonPropertySyntax> jsonPropertySyntaxes)
    {
        TextEditorTextSpan = textEditorTextSpan;

        // To avoid re-evaluating the Select() for casting as (IJsonSyntax)
        // every time the ChildJsonSyntaxes getter is accessed
        // this is being done here initially on construction once.
        JsonPropertySyntaxes = jsonPropertySyntaxes
            .Select(x => (IJsonSyntax)x)
            .ToList();
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<IJsonSyntax> JsonPropertySyntaxes { get; }
    public IReadOnlyList<IJsonSyntax> ChildJsonSyntaxes => JsonPropertySyntaxes;

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.Object;
}