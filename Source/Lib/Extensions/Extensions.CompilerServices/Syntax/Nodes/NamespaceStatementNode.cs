using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class NamespaceStatementNode : ICodeBlockOwner
{
    public NamespaceStatementNode(
        SyntaxToken keywordToken,
        SyntaxToken identifierToken,
        ResourceUri resourceUri)
    {
        KeywordToken = keywordToken;
        IdentifierToken = identifierToken;
        ResourceUri = resourceUri;
    }

    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    public ResourceUri ResourceUri { get; }

    public int ParentScopeOffset { get; set; } = -1;
    public int SelfScopeOffset { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.NamespaceStatementNode;
}
