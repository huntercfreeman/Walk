using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ReturnStatementNode : IExpressionNode
{
	public ReturnStatementNode(SyntaxToken keywordToken, IExpressionNode expressionNode)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ReturnStatementNode++;
		#endif
	
		KeywordToken = keywordToken;
		ExpressionNode = expressionNode;
	}

	public SyntaxToken KeywordToken { get; }
	public IExpressionNode ExpressionNode { get; set; }
	public TypeReference ResultTypeReference => ExpressionNode.ResultTypeReference;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ReturnStatementNode;

	#if DEBUG	
	~ReturnStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ReturnStatementNode--;
	}
	#endif
}