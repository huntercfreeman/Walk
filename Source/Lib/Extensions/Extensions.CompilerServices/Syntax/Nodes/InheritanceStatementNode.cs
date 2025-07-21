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

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.InheritanceStatementNode;

#if DEBUG
	~InheritanceStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.InheritanceStatementNode--;
	}
	#endif
}
