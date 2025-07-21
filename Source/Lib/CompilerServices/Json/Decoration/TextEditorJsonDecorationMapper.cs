using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Json.Decoration;

public class TextEditorJsonDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (JsonDecorationKind)decorationByte;

        return decoration switch
        {
            JsonDecorationKind.PropertyKey => "di_te_json-property-key",
            JsonDecorationKind.String => "di_te_string-literal",
            JsonDecorationKind.Number => "di_te_number",
            JsonDecorationKind.Integer => "di_te_integer",
            JsonDecorationKind.Keyword => "di_te_keyword",
            JsonDecorationKind.LineComment => "di_te_comment",
            JsonDecorationKind.BlockComment => "di_te_comment",
            JsonDecorationKind.None => string.Empty,
            JsonDecorationKind.Null => string.Empty,
            JsonDecorationKind.Document => string.Empty,
            JsonDecorationKind.Error => string.Empty,
            _ => string.Empty,
        };
    }
}
