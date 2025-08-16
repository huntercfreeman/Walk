using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Xml.Html;

public interface IHtmlSyntaxNode : IHtmlSyntax
{
    public List<IHtmlSyntaxNode> ChildList { get; }
    public TextEditorTextSpan TextEditorTextSpan { get; }
}
