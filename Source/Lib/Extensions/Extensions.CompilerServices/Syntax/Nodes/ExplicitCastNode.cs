using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ExplicitCastNode : IExpressionNode
{
	public ExplicitCastNode(
		SyntaxToken openParenthesisToken,
		TypeReference resultTypeReference,
		SyntaxToken closeParenthesisToken)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ExplicitCastNode++;
		#endif
	
		OpenParenthesisToken = openParenthesisToken;
		ResultTypeReference = resultTypeReference;
		CloseParenthesisToken = closeParenthesisToken;
	}

	public ExplicitCastNode(SyntaxToken openParenthesisToken, TypeReference resultTypeReference)
		: this(openParenthesisToken, resultTypeReference, default)
	{
	}

	public SyntaxToken OpenParenthesisToken { get; }
	public TypeReference ResultTypeReference { get; }
	public SyntaxToken CloseParenthesisToken { get; set; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ExplicitCastNode;

	public string IdentifierText => nameof(ExplicitCastNode);

#if DEBUG
	~ExplicitCastNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ExplicitCastNode--;
	}
	#endif
}
