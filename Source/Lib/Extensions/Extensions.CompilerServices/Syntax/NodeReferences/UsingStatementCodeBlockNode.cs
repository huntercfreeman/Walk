using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class UsingStatementCodeBlockNode : ICodeBlockOwner
{
    public UsingStatementCodeBlockNode(SyntaxToken keywordToken)
    {
        KeywordToken = keywordToken;
    }

    public SyntaxToken KeywordToken { get; }

    // ICodeBlockOwner properties.
    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.UsingStatementCodeBlockNode;
}
