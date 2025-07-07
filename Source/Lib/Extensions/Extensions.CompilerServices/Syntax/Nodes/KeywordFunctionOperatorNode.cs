using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class KeywordFunctionOperatorNode : IExpressionNode
{
	public KeywordFunctionOperatorNode(SyntaxToken keywordToken, IExpressionNode expressionNodeToMakePrimary)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.KeywordFunctionOperatorNode++;
		#endif
	
		KeywordToken = keywordToken;
		ExpressionNodeToMakePrimary = expressionNodeToMakePrimary;
	}

	public SyntaxToken KeywordToken { get; }
	public IExpressionNode ExpressionNodeToMakePrimary { get; set; }
	public TypeReference ResultTypeReference => ExpressionNodeToMakePrimary.ResultTypeReference;

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.KeywordFunctionOperatorNode;

	public string IdentifierText => nameof(KeywordFunctionOperatorNode);

#if DEBUG
	~KeywordFunctionOperatorNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.KeywordFunctionOperatorNode--;
	}
	#endif
}
