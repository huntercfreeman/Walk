namespace Walk.Extensions.CompilerServices.Syntax;

// https://code-maze.com/csharp-using-trie-class-for-efficient-text-pattern-searching/
public class NamespacePrefixNode
{
    public NamespacePrefixNode()
    {
        #if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespacePrefixNode++;
		#endif
    }

    public int Links { get; set; }
    public Dictionary<string, NamespacePrefixNode> Children { get; } = new();
    
    #if DEBUG	
	~NamespacePrefixNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespacePrefixNode--;
	}
	#endif
}
