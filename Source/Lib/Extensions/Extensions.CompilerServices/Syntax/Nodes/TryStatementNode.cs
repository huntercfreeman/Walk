using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class TryStatementNode : ISyntaxNode
{
	public TryStatementNode(
		TryStatementTryNode? tryNode,
		TryStatementCatchNode? catchNode,
		TryStatementFinallyNode? finallyNode)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TryStatementNode++;
		#endif
	
		TryNode = tryNode;
		CatchNode = catchNode;
		FinallyNode = finallyNode;
	}

	public TryStatementTryNode? TryNode { get; set; }
	public TryStatementCatchNode? CatchNode { get; set; }
	public TryStatementFinallyNode? FinallyNode { get; set; }

	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.TryStatementNode;

	public string IdentifierText => nameof(TryStatementNode);

#if DEBUG
	~TryStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TryStatementNode--;
	}
	#endif
}
