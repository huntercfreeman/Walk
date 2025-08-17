namespace Walk.CompilerServices.Xml;

public enum XmlDecorationKind
{
    None,
    AttributeName,
    AttributeValue,
    TagNameNone,
    TagNameOpen,
    TagNameClose,
    TagNameSelf,
    Comment,
    CustomTagName,
    EntityReference,
    HtmlCode,
    InjectedLanguageFragment,
    InjectedLanguageComponent,
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
