using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class CollectionInitializationNode : IExpressionNode
{
    public int ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.CollectionInitializationNode;
    
    public TypeReference ResultTypeReference { get; }
    
    public bool IsClosed { get; set; }
}
