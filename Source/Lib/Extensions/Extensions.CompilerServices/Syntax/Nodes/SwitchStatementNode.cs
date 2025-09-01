using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class SwitchStatementNode : ICodeBlockOwner
{
    public SwitchStatementNode(
        SyntaxToken keywordToken,
        SyntaxToken openParenthesisToken,
        IExpressionNode expressionNode,
        SyntaxToken closeParenthesisToken)
    {
        KeywordToken = keywordToken;
        OpenParenthesisToken = openParenthesisToken;
        ExpressionNode = expressionNode;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken KeywordToken { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public IExpressionNode ExpressionNode { get; }
    public SyntaxToken CloseParenthesisToken { get; }

    public int ParentIndexKey { get; set; } = -1;
    public int SelfIndexKey { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.SwitchStatementNode;
}
