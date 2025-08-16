using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;

namespace Walk.CompilerServices.Xml.Html.SyntaxActors;

public class HtmlSyntaxWalker : XmlSyntaxWalker
{
    public List<InjectedLanguageFragmentNode> InjectedLanguageFragmentNodes { get; } = new();
    public List<TagNode> TagOpeningNodes { get; } = new();
    public List<TagNode> TagClosingNodes { get; } = new();
    public List<TagNode> TagSelfClosingNodes { get; } = new();

    public override void VisitInjectedLanguageFragmentNode(InjectedLanguageFragmentNode node)
    {
        InjectedLanguageFragmentNodes.Add(node);
    }

    public override void VisitTagOpeningNode(TagNode node)
    {
        TagOpeningNodes.Add(node);
    }

    public override void VisitTagClosingNode(TagNode node)
    {
        TagClosingNodes.Add(node);
    }

    public override void VisitTagSelfClosingNode(TagNode node)
    {
        TagSelfClosingNodes.Add(node);
    }
}
