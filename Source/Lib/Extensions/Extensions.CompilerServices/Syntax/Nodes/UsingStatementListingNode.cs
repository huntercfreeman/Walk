namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class UsingStatementListingNode : ISyntaxNode
{
	public UsingStatementListingNode()
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.UsingStatementListingNode++;
		#endif
	}

	/// <summary>
	/// Note: don't store as ISyntax in order to avoid boxing.
	/// If someone explicitly invokes 'GetChildList()' then box at that point
	/// but 'GetChildList()' is far less likely to be invoked for this type.
	/// </summary>
	public List<(SyntaxToken KeywordToken, SyntaxToken NamespaceIdentifier)> UsingStatementTupleList { get; set; } = new();

	public int ParentScopeIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.UsingStatementListingNode;

	public string IdentifierText => nameof(UsingStatementListingNode);

#if DEBUG
	~UsingStatementListingNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.UsingStatementListingNode--;
	}
	#endif
}