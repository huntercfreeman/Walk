using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class WithExpressionNode : IExpressionNode
{
    public WithExpressionNode(VariableReferenceValue variableReference)
    {
        VariableReference = variableReference;
        ResultTypeReference = variableReference.ResultTypeReference;
    }

    public VariableReferenceValue VariableReference { get; }
    public TypeReferenceValue ResultTypeReference { get; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.WithExpressionNode;
}
