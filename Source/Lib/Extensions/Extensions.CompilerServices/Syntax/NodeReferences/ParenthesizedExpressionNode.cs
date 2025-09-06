using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class ParenthesizedExpressionNode : IExpressionNode
{
    public ParenthesizedExpressionNode(
        SyntaxToken openParenthesisToken,
        IExpressionNode innerExpression,
        SyntaxToken closeParenthesisToken)
    {
        OpenParenthesisToken = openParenthesisToken;
        InnerExpression = innerExpression;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public ParenthesizedExpressionNode(SyntaxToken openParenthesisToken, TypeReferenceValue typeReference)
        : this(openParenthesisToken, EmptyExpressionNode.Empty, default)
    {
    }

    public SyntaxToken OpenParenthesisToken { get; }
    public IExpressionNode InnerExpression { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    public TypeReferenceValue ResultTypeReference => InnerExpression.ResultTypeReference;

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ParenthesizedExpressionNode;
}
