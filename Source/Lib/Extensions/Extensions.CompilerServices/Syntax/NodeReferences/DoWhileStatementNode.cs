using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class DoWhileStatementNode : ICodeBlockOwner
{
    public DoWhileStatementNode(
        SyntaxToken doKeywordToken,
        SyntaxToken whileKeywordToken,
        SyntaxToken openParenthesisToken,
        SyntaxToken closeParenthesisToken)
    {
        DoKeywordToken = doKeywordToken;
        WhileKeywordToken = whileKeywordToken;
        OpenParenthesisToken = openParenthesisToken;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken DoKeywordToken { get; }
    public SyntaxToken WhileKeywordToken { get; set; }
    public SyntaxToken OpenParenthesisToken { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.DoWhileStatementNode;
}
