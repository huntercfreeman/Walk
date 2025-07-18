using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class InterpolatedStringNode : IExpressionNode
{
	/// <summary>
	/// The expression primary is set aside for a moment in order to parse
	/// the interpolated expressions.
	///
	/// Thus, pass it to the constructor so it can be restored as the expression primary
	/// after parsing the interpolated expressions.
	///
	/// (for example a BinaryExpressionNode might in reality be the true expression primary,
	///  but the InterpolatedStringNode is made expression primary for a time to parse its interpolated expressions first).
	///
	/// If 'toBeExpressionPrimary' is null then the 'InterpolatedStringNode' itself is the to be expression primary.
	/// </summary>
	public InterpolatedStringNode(
		SyntaxToken stringInterpolatedStartToken,
		SyntaxToken stringInterpolatedEndToken,
		IExpressionNode? toBeExpressionPrimary,
		TypeReference resultTypeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.InterpolatedStringNode++;
		#endif
	
		StringInterpolatedStartToken = stringInterpolatedStartToken;
		StringInterpolatedEndToken = stringInterpolatedEndToken;
		ToBeExpressionPrimary = toBeExpressionPrimary;
		ResultTypeReference = resultTypeReference;
	}

	public SyntaxToken StringInterpolatedStartToken { get; }
	public SyntaxToken StringInterpolatedEndToken { get; set; }

	/// <summary>
	/// If 'ToBeExpressionPrimary' is null then the 'InterpolatedStringNode' itself is the to be expression primary.
	/// </summary>
	public IExpressionNode? ToBeExpressionPrimary { get; }

	public TypeReference ResultTypeReference { get; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.InterpolatedStringNode;

#if DEBUG
	~InterpolatedStringNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.InterpolatedStringNode--;
	}
	#endif
}
