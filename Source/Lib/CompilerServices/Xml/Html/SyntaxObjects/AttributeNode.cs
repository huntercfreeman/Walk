using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;

namespace Walk.CompilerServices.Xml.Html.SyntaxObjects;

public class AttributeNode : IHtmlSyntaxNode
{
    public AttributeNode(
        AttributeNameNode attributeNameSyntax,
        AttributeValueNode attributeValueSyntax,
        TextEditorTextSpan textSpan)
    {
        AttributeNameSyntax = attributeNameSyntax;
        AttributeValueSyntax = attributeValueSyntax;
        TextEditorTextSpan = textSpan;

        ChildContent = new List<IHtmlSyntax>
        {
            AttributeNameSyntax,
            AttributeValueSyntax,
        };

        Children = ChildContent;
    }

    public AttributeNameNode AttributeNameSyntax { get; }
    public AttributeValueNode AttributeValueSyntax { get; }

    public IReadOnlyList<IHtmlSyntax> ChildContent { get; }
    public IReadOnlyList<IHtmlSyntax> Children { get; }

    public TextEditorTextSpan TextEditorTextSpan { get; }

    public HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.AttributeNode;
}
