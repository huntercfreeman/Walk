using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class ExplicitCastNode : IExpressionNode
{
    public ExplicitCastNode(
        SyntaxToken openParenthesisToken,
        TypeReferenceValue resultTypeReference,
        SyntaxToken closeParenthesisToken)
    {
        OpenParenthesisToken = openParenthesisToken;
        ResultTypeReference = resultTypeReference;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public ExplicitCastNode(SyntaxToken openParenthesisToken, TypeReferenceValue resultTypeReference)
        : this(openParenthesisToken, resultTypeReference, default)
    {
    }

    public SyntaxToken OpenParenthesisToken { get; }
    public TypeReferenceValue ResultTypeReference { get; }
    public SyntaxToken CloseParenthesisToken { get; set; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ExplicitCastNode;
}
