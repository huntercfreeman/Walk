using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// One usage of the <see cref="EmptyExpressionNode"/> is for a <see cref="ParenthesizedExpressionNode"/>
/// which has no <see cref="ParenthesizedExpressionNode.InnerExpression"/>
/// </summary>
public sealed class EmptyExpressionNode : IExpressionNode
{
	/// <summary>
	/// Walk.CompilerServices.CSharp.ParserCase.Internals.ParseOthers currently
	/// compares memory locations of nodes in 'public static IExpressionNode ParseExpression(CSharpParserModel model)'
	///
	/// So, if one uses this in conjunction with adding it to the IParserModel.ExpressionList then
	/// two it might result in discarding more entries in the list than was expected.
	///
	/// If this is used justs temporarily when instantiating a node, and never added to the
	/// IParserModel.ExpressionList then there should not be an issue.
	/// </summary>
	public static readonly EmptyExpressionNode Empty = new EmptyExpressionNode(TypeFacts.Empty.ToTypeReference());

	/// <inheritdoc cref="Empty"/>
	public static readonly EmptyExpressionNode EmptyFollowsMemberAccessToken = new EmptyExpressionNode(TypeFacts.Empty.ToTypeReference())
	{
		FollowsMemberAccessToken = true
	};

	public EmptyExpressionNode(TypeReference typeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.EmptyExpressionNode++;
		#endif
	
		ResultTypeReference = typeReference;
	}

	public TypeReference ResultTypeReference { get; }
	public bool FollowsMemberAccessToken { get; init; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.EmptyExpressionNode;

#if DEBUG
	~EmptyExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.EmptyExpressionNode--;
	}
	#endif
}