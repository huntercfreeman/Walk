using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;

namespace Walk.CompilerServices.Xml.Html.SyntaxActors;

public class HtmlSyntaxWalker : XmlSyntaxWalker
{
    public List<TextEditorTextSpan> CommentList { get; set; } = new();
    public List<TextEditorTextSpan> AttributeValueList { get; set; } = new();
    public List<TextEditorTextSpan> AttributeNameList { get; set; } = new();
    public List<TextEditorTextSpan> NameTextSpan { get; set; } = new();
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
        
        if (node.OpenNameTextSpan != default)
            NameTextSpan.Add(node.OpenNameTextSpan);
        
        if (node.CloseNameTextSpan != default)
            NameTextSpan.Add(node.CloseNameTextSpan);
        
        CommentList.AddRange(node.TextSpanList);
        AttributeNameList.AddRange(node.AttributeEntryList.Select(x => x.NameTextSpan));
        AttributeNameList.AddRange(node.AttributeEntryList.Select(x => x.ValueTextSpan));
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
