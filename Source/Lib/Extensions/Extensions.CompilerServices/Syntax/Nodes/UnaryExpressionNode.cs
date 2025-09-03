using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class UnaryExpressionNode : IExpressionNode
{
    public UnaryExpressionNode(
        IExpressionNode expression,
        UnaryOperatorNode unaryOperatorNode)
    {
        Expression = expression;
        UnaryOperatorNode = unaryOperatorNode;
    }

    public IExpressionNode Expression { get; }
    public UnaryOperatorNode UnaryOperatorNode { get; }
    public TypeReference ResultTypeReference => UnaryOperatorNode.ResultTypeReference;

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.UnaryExpressionNode;
}
