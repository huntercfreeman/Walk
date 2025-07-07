using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LiteralExpressionNode : IExpressionNode
{
	public LiteralExpressionNode(SyntaxToken literalSyntaxToken, TypeReference typeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LiteralExpressionNode++;
		#endif
	
		LiteralSyntaxToken = literalSyntaxToken;
		ResultTypeReference = typeReference;
	}

	public SyntaxToken LiteralSyntaxToken { get; }
	public TypeReference ResultTypeReference { get; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.LiteralExpressionNode;

	public string IdentifierText => nameof(LiteralExpressionNode);

#if DEBUG
	~LiteralExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LiteralExpressionNode--;
	}
	#endif
}
