using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class TupleExpressionNode : IExpressionNode
{
	public TupleExpressionNode()
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TupleExpressionNode++;
		#endif
	}

	public TypeReference ResultTypeReference { get; } = TypeFacts.Empty.ToTypeReference();

	// public List<IExpressionNode> InnerExpressionList { get; } = new();

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.TupleExpressionNode;

#if DEBUG
	~TupleExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TupleExpressionNode--;
	}
	#endif
}
