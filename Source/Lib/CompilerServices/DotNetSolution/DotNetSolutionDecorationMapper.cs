using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.DotNetSolution;

public class DotNetSolutionDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (DotNetSolutionDecorationKind)decorationByte;

        return decoration switch
        {
            DotNetSolutionDecorationKind.None => string.Empty,
            DotNetSolutionDecorationKind.AttributeName => "di_attribute-name",
            DotNetSolutionDecorationKind.AttributeValue => "di_attribute-value",
            DotNetSolutionDecorationKind.AttributeOperator => "di_attribute-value",
            DotNetSolutionDecorationKind.AttributeDelimiter => "di_attribute-value",
            DotNetSolutionDecorationKind.AttributeNameInjectedLanguageFragment => "di_injected-language-fragment",
            DotNetSolutionDecorationKind.AttributeValueInjectedLanguageFragment => "di_injected-language-fragment",
            DotNetSolutionDecorationKind.AttributeValueInterpolationStart => "di_attribute-value",
            DotNetSolutionDecorationKind.AttributeValueInterpolationContinue => "di_attribute-value",
            DotNetSolutionDecorationKind.TagNameNone => "di_tag-name",
            DotNetSolutionDecorationKind.TagNameOpen => "di_tag-name",
            DotNetSolutionDecorationKind.TagNameClose => "di_tag-name",
            DotNetSolutionDecorationKind.TagNameSelf => "di_tag-name",
            DotNetSolutionDecorationKind.Comment => "di_comment",
            DotNetSolutionDecorationKind.CustomTagName => "di_te_custom-tag-name",
            DotNetSolutionDecorationKind.EntityReference => "di_te_entity-reference",
            DotNetSolutionDecorationKind.HtmlCode => "di_te_html-code",
            DotNetSolutionDecorationKind.InjectedLanguageFragment => "di_injected-language-fragment",
            DotNetSolutionDecorationKind.InjectedLanguageComponent => "di_injected-language-component",
            DotNetSolutionDecorationKind.CSharpMarker => "di_type",
            DotNetSolutionDecorationKind.Error => "di_te_error",
            DotNetSolutionDecorationKind.InjectedLanguageCodeBlock => "di_te_injected-language-code-block",
            DotNetSolutionDecorationKind.InjectedLanguageCodeBlockTag => "di_te_injected-language-code-block-tag",
            DotNetSolutionDecorationKind.InjectedLanguageKeyword => "di_keyword",
            DotNetSolutionDecorationKind.InjectedLanguageTagHelperAttribute => "di_te_injected-language-tag-helper-attribute",
            DotNetSolutionDecorationKind.InjectedLanguageTagHelperElement => "di_te_injected-language-tag-helper-element",
            DotNetSolutionDecorationKind.InjectedLanguageMethod => "di_method",
            DotNetSolutionDecorationKind.InjectedLanguageVariable => "di_variable",
            DotNetSolutionDecorationKind.InjectedLanguageType => "di_type",
            DotNetSolutionDecorationKind.InjectedLanguageStringLiteral => "di_string",
            _ => string.Empty,
        };
    }
}
