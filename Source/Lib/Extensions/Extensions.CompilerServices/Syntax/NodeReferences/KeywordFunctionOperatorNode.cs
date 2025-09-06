using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class KeywordFunctionOperatorNode : IExpressionNode
{
    public KeywordFunctionOperatorNode(SyntaxToken keywordToken, IExpressionNode expressionNodeToMakePrimary)
    {
        KeywordToken = keywordToken;
        ExpressionNodeToMakePrimary = expressionNodeToMakePrimary;
    }

    public SyntaxToken KeywordToken { get; }
    public IExpressionNode ExpressionNodeToMakePrimary { get; set; }
    public TypeReferenceValue ResultTypeReference => ExpressionNodeToMakePrimary.ResultTypeReference;

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.KeywordFunctionOperatorNode;
}
