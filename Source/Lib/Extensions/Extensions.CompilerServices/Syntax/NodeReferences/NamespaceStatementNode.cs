using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;

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

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.NamespaceStatementNode;
}
