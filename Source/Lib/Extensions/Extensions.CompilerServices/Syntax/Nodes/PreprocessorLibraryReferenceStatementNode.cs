namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class PreprocessorLibraryReferenceStatementNode : ISyntaxNode
{
	public PreprocessorLibraryReferenceStatementNode(
		SyntaxToken includeDirectiveSyntaxToken,
		SyntaxToken libraryReferenceSyntaxToken)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.PreprocessorLibraryReferenceStatementNode++;
		#endif
	
		IncludeDirectiveSyntaxToken = includeDirectiveSyntaxToken;
		LibraryReferenceSyntaxToken = libraryReferenceSyntaxToken;
	}

	public SyntaxToken IncludeDirectiveSyntaxToken { get; }
	public SyntaxToken LibraryReferenceSyntaxToken { get; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.PreprocessorLibraryReferenceStatementNode;

#if DEBUG
	~PreprocessorLibraryReferenceStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.PreprocessorLibraryReferenceStatementNode--;
	}
	#endif
}