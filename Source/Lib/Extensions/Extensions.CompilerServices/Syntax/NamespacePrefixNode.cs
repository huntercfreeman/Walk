using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax;

// https://code-maze.com/csharp-using-trie-class-for-efficient-text-pattern-searching/
public class NamespacePrefixNode
{
    public NamespacePrefixNode(ResourceUri resourceUri, TextEditorTextSpan textSpan)
    {
        ResourceUri = resourceUri;
        TextSpan = textSpan;
    }

    public int Links { get; set; }
    public TextEditorTextSpan TextSpan { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public List<NamespacePrefixNode> Children { get; } = new();
}
