using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class KeywordFunctionOperatorNode : IExpressionNode
{
    public KeywordFunctionOperatorNode(SyntaxToken keywordToken, IExpressionNode expressionNodeToMakePrimary)
    {
        KeywordToken = keywordToken;
        ExpressionNodeToMakePrimary = expressionNodeToMakePrimary;
    }

    public SyntaxToken KeywordToken { get; }
    public IExpressionNode ExpressionNodeToMakePrimary { get; set; }
    public TypeReference ResultTypeReference => ExpressionNodeToMakePrimary.ResultTypeReference;

    public int Unsafe_ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.KeywordFunctionOperatorNode;
}
