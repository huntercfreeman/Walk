using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Css.Decoration;

public class TextEditorCssDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (CssDecorationKind)decorationByte;

        return decoration switch
        {
            CssDecorationKind.None => string.Empty,
            CssDecorationKind.Identifier => "di_css-identifier",
            CssDecorationKind.PropertyName => "di_css-property-name",
            CssDecorationKind.PropertyValue => "di_css-property-value",
            CssDecorationKind.Comment => "di_comment",
            _ => string.Empty,
        };
    }
}
