using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

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

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.WithExpressionNode;
}
