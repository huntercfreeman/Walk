using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Json.SyntaxEnums;

namespace Walk.CompilerServices.Json.SyntaxObjects;

public class JsonPropertySyntax : IJsonSyntax
{
    public JsonPropertySyntax(
        TextEditorTextSpan textEditorTextSpan,
        JsonPropertyKeySyntax jsonPropertyKeySyntax,
        JsonPropertyValueSyntax jsonPropertyValueSyntax)
    {
        TextEditorTextSpan = textEditorTextSpan;
        JsonPropertyKeySyntax = jsonPropertyKeySyntax;
        JsonPropertyValueSyntax = jsonPropertyValueSyntax;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public JsonPropertyKeySyntax JsonPropertyKeySyntax { get; }
    public JsonPropertyValueSyntax JsonPropertyValueSyntax { get; }
    public IReadOnlyList<IJsonSyntax> ChildJsonSyntaxes => new List<IJsonSyntax>
    {
        JsonPropertyKeySyntax,
        JsonPropertyValueSyntax
    };

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.Property;
}