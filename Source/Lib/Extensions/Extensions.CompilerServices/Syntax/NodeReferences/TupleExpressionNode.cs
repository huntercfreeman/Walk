using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class TupleExpressionNode : IExpressionNode
{
    public TypeReferenceValue ResultTypeReference { get; } = TypeFacts.Empty.ToTypeReference();

    // public List<IExpressionNode> InnerExpressionList { get; } = new();

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.TupleExpressionNode;
}
