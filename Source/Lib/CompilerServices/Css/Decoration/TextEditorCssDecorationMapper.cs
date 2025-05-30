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
            CssDecorationKind.Identifier => "di_te_css-identifier",
            CssDecorationKind.PropertyName => "di_te_css-property-name",
            CssDecorationKind.PropertyValue => "di_te_css-property-value",
            CssDecorationKind.Comment => "di_te_comment",
            _ => string.Empty,
        };
    }
}