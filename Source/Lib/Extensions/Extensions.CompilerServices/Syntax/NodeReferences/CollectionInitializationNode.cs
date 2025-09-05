using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class CollectionInitializationNode : IExpressionNode
{
    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.CollectionInitializationNode;
    
    public TypeReferenceValue ResultTypeReference { get; }
    
    public bool IsClosed { get; set; }
}
