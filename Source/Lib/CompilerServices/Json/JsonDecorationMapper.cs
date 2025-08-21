using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Json;

public class JsonDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (JsonDecorationKind)decorationByte;

        return decoration switch
        {
            JsonDecorationKind.PropertyKey => "di_json-property-key",
            JsonDecorationKind.String => "di_string",
            JsonDecorationKind.Number => "di_te_number",
            JsonDecorationKind.Integer => "di_te_integer",
            JsonDecorationKind.Keyword => "di_keyword",
            JsonDecorationKind.LineComment => "di_comment",
            JsonDecorationKind.BlockComment => "di_comment",
            JsonDecorationKind.None => string.Empty,
            JsonDecorationKind.Null => string.Empty,
            JsonDecorationKind.Document => string.Empty,
            JsonDecorationKind.Error => string.Empty,
            _ => string.Empty,
        };
    }
}
