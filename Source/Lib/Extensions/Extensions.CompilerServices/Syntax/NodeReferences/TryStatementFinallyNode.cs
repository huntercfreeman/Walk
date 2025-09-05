using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class TryStatementFinallyNode : ICodeBlockOwner
{
    public TryStatementFinallyNode(SyntaxToken keywordToken)
    {
        KeywordToken = keywordToken;
    }

    public SyntaxToken KeywordToken { get; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.TryStatementFinallyNode;
}
