using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ForStatementNode : ICodeBlockOwner
{
    public ForStatementNode(
        SyntaxToken keywordToken,
        SyntaxToken openParenthesisToken,
        SyntaxToken initializationStatementDelimiterToken,
        SyntaxToken conditionStatementDelimiterToken,
        SyntaxToken closeParenthesisToken)
    {
        KeywordToken = keywordToken;
        OpenParenthesisToken = openParenthesisToken;
        InitializationStatementDelimiterToken = initializationStatementDelimiterToken;
        ConditionStatementDelimiterToken = conditionStatementDelimiterToken;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken KeywordToken { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SyntaxToken InitializationStatementDelimiterToken { get; }
    public SyntaxToken ConditionStatementDelimiterToken { get; }
    public SyntaxToken CloseParenthesisToken { get; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ForStatementNode;
}
