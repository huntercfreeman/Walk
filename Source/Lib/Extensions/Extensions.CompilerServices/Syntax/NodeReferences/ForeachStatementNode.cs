using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ForeachStatementNode : ICodeBlockOwner
{
    public ForeachStatementNode(
        SyntaxToken foreachKeywordToken,
        SyntaxToken openParenthesisToken,
        SyntaxToken inKeywordToken,
        SyntaxToken closeParenthesisToken)
    {
        ForeachKeywordToken = foreachKeywordToken;
        OpenParenthesisToken = openParenthesisToken;
        InKeywordToken = inKeywordToken;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken ForeachKeywordToken { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SyntaxToken InKeywordToken { get; }
    public SyntaxToken CloseParenthesisToken { get; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ForeachStatementNode;
}
