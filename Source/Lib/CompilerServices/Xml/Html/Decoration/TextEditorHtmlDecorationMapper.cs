using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Xml.Html.Decoration;

public class TextEditorHtmlDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (HtmlDecorationKind)decorationByte;

        return decoration switch
        {
            HtmlDecorationKind.None => string.Empty,
            HtmlDecorationKind.AttributeName => "di_te_attribute-name",
            HtmlDecorationKind.AttributeValue => "di_te_attribute-value",
            HtmlDecorationKind.Comment => "di_te_comment",
            HtmlDecorationKind.CustomTagName => "di_te_custom-tag-name",
            HtmlDecorationKind.EntityReference => "di_te_entity-reference",
            HtmlDecorationKind.HtmlCode => "di_te_html-code",
            HtmlDecorationKind.InjectedLanguageFragment => "di_te_injected-language-fragment",
            HtmlDecorationKind.InjectedLanguageComponent => "di_te_injected-language-component",
            HtmlDecorationKind.TagName => "di_te_tag-name",
            HtmlDecorationKind.Tag => "di_te_tag",
            HtmlDecorationKind.Error => "di_te_error",
            HtmlDecorationKind.InjectedLanguageCodeBlock => "di_te_injected-language-code-block",
            HtmlDecorationKind.InjectedLanguageCodeBlockTag => "di_te_injected-language-code-block-tag",
            HtmlDecorationKind.InjectedLanguageKeyword => "di_te_keyword",
            HtmlDecorationKind.InjectedLanguageTagHelperAttribute => "di_te_injected-language-tag-helper-attribute",
            HtmlDecorationKind.InjectedLanguageTagHelperElement => "di_te_injected-language-tag-helper-element",
            HtmlDecorationKind.InjectedLanguageMethod => "di_te_method",
            HtmlDecorationKind.InjectedLanguageVariable => "di_te_variable",
            HtmlDecorationKind.InjectedLanguageType => "di_te_type",
            HtmlDecorationKind.InjectedLanguageStringLiteral => "di_te_string-literal",
            _ => string.Empty,
        };
    }
}
