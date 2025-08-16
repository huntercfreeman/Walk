using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;

namespace Walk.CompilerServices.Xml.Html.SyntaxObjects;

public class TagNode : IHtmlSyntaxNode
{
    public TextEditorTextSpan OpenNameTextSpan { get; set; }
    public TextEditorTextSpan CloseNameTextSpan { get; set; }
    public List<AttributeEntry> AttributeEntryList { get; set; }
    public List<IHtmlSyntaxNode> ChildList { get; set; }
    public List<TextEditorTextSpan> TextSpanList { get; set; }
    public HtmlSyntaxKind HtmlSyntaxKind { get; set; }
    public bool HasSpecialHtmlCharacter { get; set; }

    public TextEditorTextSpan TextEditorTextSpan => new(
        0,
        0,
        (byte)GenericDecorationKind.None);
}
