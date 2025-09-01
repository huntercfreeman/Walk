namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class PreprocessorLibraryReferenceStatementNode : ISyntaxNode
{
    public PreprocessorLibraryReferenceStatementNode(
        SyntaxToken includeDirectiveSyntaxToken,
        SyntaxToken libraryReferenceSyntaxToken)
    {
        IncludeDirectiveSyntaxToken = includeDirectiveSyntaxToken;
        LibraryReferenceSyntaxToken = libraryReferenceSyntaxToken;
    }

    public SyntaxToken IncludeDirectiveSyntaxToken { get; }
    public SyntaxToken LibraryReferenceSyntaxToken { get; }

    public int ParentScopeOffset { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.PreprocessorLibraryReferenceStatementNode;
}
