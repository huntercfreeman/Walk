using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LockStatementNode : ICodeBlockOwner
{
    public LockStatementNode(
        SyntaxToken keywordToken,
        SyntaxToken openParenthesisToken,
        SyntaxToken closeParenthesisToken)
    {
        KeywordToken = keywordToken;
        OpenParenthesisToken = openParenthesisToken;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken KeywordToken { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SyntaxToken CloseParenthesisToken { get; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LockStatementNode;
}
