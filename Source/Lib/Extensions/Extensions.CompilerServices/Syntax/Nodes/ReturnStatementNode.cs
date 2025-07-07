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
		// ExpressionNode = expressionNode;
		
		ResultTypeReference = expressionNode.ResultTypeReference;
	}

	public SyntaxToken KeywordToken { get; }
	// public IExpressionNode ExpressionNode { get; set; }
	public TypeReference ResultTypeReference { get; }

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ReturnStatementNode;

	public string IdentifierText => nameof(ReturnStatementNode);

#if DEBUG
	~ReturnStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ReturnStatementNode--;
	}
	#endif
}