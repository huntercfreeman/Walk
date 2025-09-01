using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
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

    public int ParentScopeOffset { get; set; } = -1;
    public int SelfScopeOffset { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ForStatementNode;
}
