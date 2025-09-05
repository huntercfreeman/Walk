using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class UnaryOperatorNode : ISyntaxNode
{
    public UnaryOperatorNode(
        TypeReferenceValue operandTypeReference,
        SyntaxToken operatorToken,
        TypeReferenceValue resultTypeReference)
    {
        OperandTypeReference = operandTypeReference;
        OperatorToken = operatorToken;
        ResultTypeReference = resultTypeReference;
    }

    public TypeReferenceValue OperandTypeReference { get; }
    public SyntaxToken OperatorToken { get; }
    public TypeReferenceValue ResultTypeReference { get; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.UnaryOperatorNode;
}
