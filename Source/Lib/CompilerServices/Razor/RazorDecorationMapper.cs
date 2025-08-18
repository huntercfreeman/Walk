using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Razor;

public class RazorDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (RazorDecorationKind)decorationByte;

        return decoration switch
        {
            RazorDecorationKind.None => string.Empty,
            RazorDecorationKind.AttributeName => "di_attribute-name",
            RazorDecorationKind.AttributeValue => "di_attribute-value",
            RazorDecorationKind.AttributeOperator => "di_attribute-value",
            RazorDecorationKind.AttributeDelimiter => "di_attribute-value",
            RazorDecorationKind.AttributeNameInjectedLanguageFragment => "di_injected-language-fragment",
            RazorDecorationKind.AttributeValueInjectedLanguageFragment => "di_injected-language-fragment",
            RazorDecorationKind.TagNameNone => "di_tag-name",
            RazorDecorationKind.TagNameOpen => "di_tag-name",
            RazorDecorationKind.TagNameClose => "di_tag-name",
            RazorDecorationKind.TagNameSelf => "di_tag-name",
            RazorDecorationKind.Comment => "di_comment",
            RazorDecorationKind.CustomTagName => "di_te_custom-tag-name",
            RazorDecorationKind.EntityReference => "di_te_entity-reference",
            RazorDecorationKind.HtmlCode => "di_te_html-code",
            RazorDecorationKind.InjectedLanguageFragment => "di_injected-language-fragment",
            RazorDecorationKind.InjectedLanguageComponent => "di_injected-language-component",
            RazorDecorationKind.Error => "di_te_error",
            RazorDecorationKind.InjectedLanguageCodeBlock => "di_te_injected-language-code-block",
            RazorDecorationKind.InjectedLanguageCodeBlockTag => "di_te_injected-language-code-block-tag",
            RazorDecorationKind.InjectedLanguageKeyword => "di_keyword",
            RazorDecorationKind.InjectedLanguageTagHelperAttribute => "di_te_injected-language-tag-helper-attribute",
            RazorDecorationKind.InjectedLanguageTagHelperElement => "di_te_injected-language-tag-helper-element",
            RazorDecorationKind.InjectedLanguageMethod => "di_method",
            RazorDecorationKind.InjectedLanguageVariable => "di_variable",
            RazorDecorationKind.InjectedLanguageType => "di_type",
            RazorDecorationKind.InjectedLanguageStringLiteral => "di_string",
            _ => string.Empty,
        };
    }
}
