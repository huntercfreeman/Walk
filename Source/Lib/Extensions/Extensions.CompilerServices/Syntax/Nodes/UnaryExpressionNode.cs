using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class UnaryExpressionNode : IExpressionNode
{
	public UnaryExpressionNode(
		IExpressionNode expression,
		UnaryOperatorNode unaryOperatorNode)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.UnaryExpressionNode++;
		#endif
	
		Expression = expression;
		UnaryOperatorNode = unaryOperatorNode;
	}

	public IExpressionNode Expression { get; }
	public UnaryOperatorNode UnaryOperatorNode { get; }
	public TypeReference ResultTypeReference => UnaryOperatorNode.ResultTypeReference;

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.UnaryExpressionNode;

	public string IdentifierText => nameof(UnaryExpressionNode);

#if DEBUG
	~UnaryExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.UnaryExpressionNode--;
	}
	#endif
}