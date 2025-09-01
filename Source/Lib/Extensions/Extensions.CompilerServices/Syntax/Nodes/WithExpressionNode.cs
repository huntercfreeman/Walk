using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class WithExpressionNode : IExpressionNode
{
    public WithExpressionNode(VariableReference variableReference)
    {
        VariableReference = variableReference;
        ResultTypeReference = variableReference.ResultTypeReference;
    }

    public VariableReference VariableReference { get; }
    public TypeReference ResultTypeReference { get; }

    public int ParentScopeOffset { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.WithExpressionNode;
}
