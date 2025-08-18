using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Xml.Html.Decoration;

public class XmlDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (XmlDecorationKind)decorationByte;

        return decoration switch
        {
            XmlDecorationKind.None => string.Empty,
            XmlDecorationKind.AttributeName => "di_attribute-name",
            XmlDecorationKind.AttributeValue => "di_attribute-value",
            XmlDecorationKind.AttributeOperator => "di_attribute-value",
            XmlDecorationKind.AttributeDelimiter => "di_attribute-value",
            XmlDecorationKind.TagNameNone => "di_tag-name",
            XmlDecorationKind.TagNameOpen => "di_tag-name",
            XmlDecorationKind.TagNameClose => "di_tag-name",
            XmlDecorationKind.TagNameSelf => "di_tag-name",
            XmlDecorationKind.Comment => "di_comment",
            XmlDecorationKind.CustomTagName => "di_te_custom-tag-name",
            XmlDecorationKind.EntityReference => "di_te_entity-reference",
            XmlDecorationKind.HtmlCode => "di_te_html-code",
            XmlDecorationKind.InjectedLanguageFragment => "di_injected-language-fragment",
            XmlDecorationKind.InjectedLanguageComponent => "di_injected-language-component",
            XmlDecorationKind.Error => "di_te_error",
            XmlDecorationKind.InjectedLanguageCodeBlock => "di_te_injected-language-code-block",
            XmlDecorationKind.InjectedLanguageCodeBlockTag => "di_te_injected-language-code-block-tag",
            XmlDecorationKind.InjectedLanguageKeyword => "di_keyword",
            XmlDecorationKind.InjectedLanguageTagHelperAttribute => "di_te_injected-language-tag-helper-attribute",
            XmlDecorationKind.InjectedLanguageTagHelperElement => "di_te_injected-language-tag-helper-element",
            XmlDecorationKind.InjectedLanguageMethod => "di_method",
            XmlDecorationKind.InjectedLanguageVariable => "di_variable",
            XmlDecorationKind.InjectedLanguageType => "di_type",
            XmlDecorationKind.InjectedLanguageStringLiteral => "di_string",
            _ => string.Empty,
        };
    }
}
