using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class UsingStatementListingNode : ISyntaxNode
{
    /// <summary>
    /// Note: don't store as ISyntax in order to avoid boxing.
    /// If someone explicitly invokes 'GetChildList()' then box at that point
    /// but 'GetChildList()' is far less likely to be invoked for this type.
    /// </summary>
    public List<(SyntaxToken KeywordToken, SyntaxToken NamespaceIdentifier)> UsingStatementTupleList { get; set; } = new();

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.UsingStatementListingNode;
}
