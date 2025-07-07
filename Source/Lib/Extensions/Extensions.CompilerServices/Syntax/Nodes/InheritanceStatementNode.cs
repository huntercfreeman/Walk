namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class InheritanceStatementNode : ISyntaxNode
{
	public InheritanceStatementNode(TypeClauseNode parentTypeClauseNode)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.InheritanceStatementNode++;
		#endif
	
		ParentTypeClauseNode = parentTypeClauseNode;
	}

	public TypeClauseNode ParentTypeClauseNode { get; }

	public int ParentScopeIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.InheritanceStatementNode;

	public string IdentifierText => nameof(InheritanceStatementNode);

#if DEBUG
	~InheritanceStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.InheritanceStatementNode--;
	}
	#endif
}