using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Json.SyntaxEnums;

namespace Walk.CompilerServices.Json.SyntaxObjects;

public class JsonPropertyValueSyntax : IJsonSyntax
{
    public JsonPropertyValueSyntax(
        TextEditorTextSpan textEditorTextSpan,
        IJsonSyntax underlyingJsonSyntax)
    {
        UnderlyingJsonSyntax = underlyingJsonSyntax;
        TextEditorTextSpan = textEditorTextSpan;
    }

    public static JsonPropertyValueSyntax GetInvalidJsonPropertyValueSyntax()
    {
        return new JsonPropertyValueSyntax(
            new TextEditorTextSpan(
                0,
                0,
                default),
            null);
    }

    public IJsonSyntax UnderlyingJsonSyntax { get; }
    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<IJsonSyntax> ChildJsonSyntaxes => new List<IJsonSyntax>
    {
        UnderlyingJsonSyntax
    };

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.PropertyValue;
}