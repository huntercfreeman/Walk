using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class BinaryExpressionNode : IExpressionNode
{
    public BinaryExpressionNode(
        TypeReference leftOperandTypeReference,
        SyntaxToken operatorToken,
        TypeReference rightOperandTypeReference,
        TypeReference resultTypeReference,
        TypeReference rightExpressionResultTypeReference)
    {
        LeftOperandTypeReference = leftOperandTypeReference;
        OperatorToken = operatorToken;
        RightOperandTypeReference = rightOperandTypeReference;
        ResultTypeReference = resultTypeReference;
        RightExpressionResultTypeReference = rightExpressionResultTypeReference;
    }

    public BinaryExpressionNode(
        TypeReference leftOperandTypeReference,
        SyntaxToken operatorToken,
        TypeReference rightOperandTypeReference,
        TypeReference resultTypeReference)
    {
        LeftOperandTypeReference = leftOperandTypeReference;
        OperatorToken = operatorToken;
        RightOperandTypeReference = rightOperandTypeReference;
        ResultTypeReference = resultTypeReference;
    }

    public TypeReference _rightExpressionResultTypeReference;
    
    public bool _isFabricated;

    public TypeReference LeftOperandTypeReference { get; set; }
    public SyntaxToken OperatorToken { get; set; }
    public TypeReference RightOperandTypeReference { get; set; }
    public TypeReference ResultTypeReference { get; set; }
    
    public TypeReference RightExpressionResultTypeReference
    {
        get => _rightExpressionResultTypeReference;
        set
        {
            _rightExpressionResultTypeReference = value;
            RightExpressionNodeWasSet = true;
        }
    }
    public bool RightExpressionNodeWasSet { get; set; }

    public int ParentIndexKey { get; set; }
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
