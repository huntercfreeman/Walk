namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class UnaryOperatorNode : ISyntaxNode
{
    public UnaryOperatorNode(
        TypeReference operandTypeReference,
        SyntaxToken operatorToken,
        TypeReference resultTypeReference)
    {
        OperandTypeReference = operandTypeReference;
        OperatorToken = operatorToken;
        ResultTypeReference = resultTypeReference;
    }

    public TypeReference OperandTypeReference { get; }
    public SyntaxToken OperatorToken { get; }
    public TypeReference ResultTypeReference { get; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.UnaryOperatorNode;
}
