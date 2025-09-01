using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ReturnStatementNode : IExpressionNode
{
    public ReturnStatementNode(SyntaxToken keywordToken, IExpressionNode expressionNode)
    {
        KeywordToken = keywordToken;
        // ExpressionNode = expressionNode;
        
        ResultTypeReference = expressionNode.ResultTypeReference;
    }

    public SyntaxToken KeywordToken { get; }
    // public IExpressionNode ExpressionNode { get; set; }
    public TypeReference ResultTypeReference { get; }

    public int ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ReturnStatementNode;
}
