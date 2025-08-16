using Walk.CompilerServices.Xml.Html.SyntaxEnums;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;

namespace Walk.CompilerServices.Xml.Html.SyntaxActors;

public abstract class XmlSyntaxWalker
{
    public virtual void Visit(IHtmlSyntaxNode node)
    {
        foreach (var child in node.ChildList)
        {
            if (child is null)
                continue;

            if (child.HtmlSyntaxKind.ToString().EndsWith("Node"))
                Visit((IHtmlSyntaxNode)child);
        }

        switch (node.HtmlSyntaxKind)
        {
            case HtmlSyntaxKind.InjectedLanguageFragmentNode:
                VisitInjectedLanguageFragmentNode((InjectedLanguageFragmentNode)node);
                break;
            case HtmlSyntaxKind.TagOpeningNode:
                VisitTagOpeningNode((TagNode)node);
                break;
            case HtmlSyntaxKind.TagClosingNode:
                VisitTagClosingNode((TagNode)node);
                break;
            case HtmlSyntaxKind.TagSelfClosingNode:
                VisitTagSelfClosingNode((TagNode)node);
                break;
        }
    }

    public virtual void VisitInjectedLanguageFragmentNode(InjectedLanguageFragmentNode node)
    {

    }

    public virtual void VisitTagOpeningNode(TagNode node)
    {

    }

    public virtual void VisitTagClosingNode(TagNode node)
    {

    }

    public virtual void VisitTagSelfClosingNode(TagNode node)
    {

    }
}
