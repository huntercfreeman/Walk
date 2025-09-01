using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class IfStatementNode : ICodeBlockOwner
{
    public IfStatementNode(SyntaxToken keywordToken)
    {
        KeywordToken = keywordToken;
    }

    public SyntaxToken KeywordToken { get; }

    public int ParentIndexKey { get; set; } = -1;
    public int SelfIndexKey { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.IfStatementNode;
}
