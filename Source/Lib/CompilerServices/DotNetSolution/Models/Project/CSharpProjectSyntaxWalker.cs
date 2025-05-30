using Walk.CompilerServices.Xml.Html.SyntaxActors;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectSyntaxWalker : XmlSyntaxWalker
{
    public List<TagNode> TagNodes { get; } = new();

    public override void VisitTagOpeningNode(TagNode node)
    {
        TagNodes.Add(node);
    }

    public override void VisitTagClosingNode(TagNode node)
    {
        TagNodes.Add(node);
    }

    public override void VisitTagSelfClosingNode(TagNode node)
    {
        TagNodes.Add(node);
    }
}