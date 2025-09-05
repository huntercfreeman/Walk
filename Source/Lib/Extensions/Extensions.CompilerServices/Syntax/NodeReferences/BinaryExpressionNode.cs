using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class BinaryExpressionNode : IExpressionNode
{
    public BinaryExpressionNode(
        TypeReferenceValue leftOperandTypeReference,
        SyntaxToken operatorToken,
        TypeReferenceValue rightOperandTypeReference,
        TypeReferenceValue resultTypeReference,
        TypeReferenceValue rightExpressionResultTypeReference)
    {
        LeftOperandTypeReference = leftOperandTypeReference;
        OperatorToken = operatorToken;
        RightOperandTypeReference = rightOperandTypeReference;
        ResultTypeReference = resultTypeReference;
        RightExpressionResultTypeReference = rightExpressionResultTypeReference;
    }

    public BinaryExpressionNode(
        TypeReferenceValue leftOperandTypeReference,
        SyntaxToken operatorToken,
        TypeReferenceValue rightOperandTypeReference,
        TypeReferenceValue resultTypeReference)
    {
        LeftOperandTypeReference = leftOperandTypeReference;
        OperatorToken = operatorToken;
        RightOperandTypeReference = rightOperandTypeReference;
        ResultTypeReference = resultTypeReference;
    }

    public TypeReferenceValue _rightExpressionResultTypeReference;
    
    public bool _isFabricated;

    public TypeReferenceValue LeftOperandTypeReference { get; set; }
    public SyntaxToken OperatorToken { get; set; }
    public TypeReferenceValue RightOperandTypeReference { get; set; }
    public TypeReferenceValue ResultTypeReference { get; set; }
    
    public TypeReferenceValue RightExpressionResultTypeReference
    {
        get => _rightExpressionResultTypeReference;
        set
        {
            _rightExpressionResultTypeReference = value;
            RightExpressionNodeWasSet = true;
        }
    }
    public bool RightExpressionNodeWasSet { get; set; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated
    {
        get
        {
            return _isFabricated;
        }
        init
        {
            _isFabricated = value;
        }
    }
    public SyntaxKind SyntaxKind => SyntaxKind.BinaryExpressionNode;
}
