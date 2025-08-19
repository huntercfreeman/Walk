namespace Walk.CompilerServices.Razor;

public enum RazorDecorationKind
{
    None,
    AttributeName,
    AttributeValue,
    AttributeValueInterpolationStart,
    AttributeValueInterpolationContinue,
    AttributeValueInterpolationEnd,
    AttributeNameInjectedLanguageFragment,
    // For when:
    //     class=@myClass
    //
    // i.e.: no deliminating double quotes or single quotes
    // but the attribute value is a C# expression.
    AttributeValueInjectedLanguageFragment,
    AttributeOperator,
    AttributeDelimiter,
    TagNameNone,
    TagNameOpen,
    TagNameClose,
    TagNameSelf,
    Comment,
    Text,
    CustomTagName,
    EntityReference,
    HtmlCode,
    InjectedLanguageFragment,
    InjectedLanguageComponent,
    CSharpMarker,
    Error,
    InjectedLanguageCodeBlock,
    InjectedLanguageCodeBlockTag,
    InjectedLanguageKeyword,
    InjectedLanguageTagHelperAttribute,
    InjectedLanguageTagHelperElement,
    InjectedLanguageMethod,
    InjectedLanguageVariable,
    InjectedLanguageType,
    InjectedLanguageStringLiteral,
}
