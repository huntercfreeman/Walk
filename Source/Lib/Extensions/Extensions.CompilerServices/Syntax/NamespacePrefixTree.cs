namespace Walk.Extensions.CompilerServices.Syntax;

// https://code-maze.com/csharp-using-trie-class-for-efficient-text-pattern-searching/
public class NamespacePrefixTree
{
    public NamespacePrefixNode __Root { get; } = new();
    
    /// <summary>
    /// When parsing a namespace statement, invoke this at each "namespace prefix" and then use the `continueNode`
    /// to restore state when you get to the next "namespace prefix".
    /// </summary>
    public NamespacePrefixNode AddNamespacePrefix(string namespacePrefix, NamespacePrefixNode? continueNode = null)
    {
        var node = continueNode ?? __Root;

        if (!node.Children.ContainsKey(namespacePrefix))
            node.Children[namespacePrefix] = new();

        node = node.Children[namespacePrefix];
        node.Links++;
        return node;
    }
}
