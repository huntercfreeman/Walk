using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;

namespace Walk.CompilerServices.Xml.Html.SyntaxObjects;

public class InjectedLanguageFragmentNode : IHtmlSyntaxNode
{
    public InjectedLanguageFragmentNode(
        List<IHtmlSyntaxNode> childList,
        TextEditorTextSpan textEditorTextSpan)
    {
        TextEditorTextSpan = textEditorTextSpan;

        ChildList = childList;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public List<IHtmlSyntaxNode> ChildList { get; }

    public HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.InjectedLanguageFragmentNode;
}
