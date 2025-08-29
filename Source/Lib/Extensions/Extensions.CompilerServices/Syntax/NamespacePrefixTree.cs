namespace Walk.Extensions.CompilerServices.Syntax;

// https://code-maze.com/csharp-using-trie-class-for-efficient-text-pattern-searching/
public class NamespacePrefixTree
{
    public NamespacePrefixNode __Root { get; } = new(default, default);

    /// <summary>(inclusive, exclusive, this is the index at which you'd insert the text span)</summary>
    public (int StartIndex, int EndIndex, int InsertionIndex) FindRange(NamespacePrefixNode startNode, int charIntSum)
    {
        var startIndex = -1;
        var endIndex = -1;
        var insertionIndex = startNode.Children.Count;
    
        for (int i = 0; i < startNode.Children.Count; i++)
        {
            var node = startNode.Children[i];
            
            if (node.TextSpan.CharIntSum == charIntSum)
            {
                if (startIndex == -1)
                    startIndex = i;
            }
            else if (startIndex != -1)
            {
                endIndex = i;
                insertionIndex = i;
                break;
            }
        }
        
        if (startIndex != -1 && endIndex == -1)
            endIndex = startNode.Children.Count;
        
        return (startIndex, endIndex, insertionIndex);
    }
}
