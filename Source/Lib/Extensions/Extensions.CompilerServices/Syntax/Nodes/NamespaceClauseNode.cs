using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class NamespaceClauseNode : IExpressionNode
{
	public NamespaceClauseNode(SyntaxToken identifierToken)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespaceClauseNode++;
		#endif
	
		IdentifierToken = identifierToken;
	}

	public SyntaxToken IdentifierToken { get; set; }
	public Type? ValueType { get; set; }

	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

	public bool IsFabricated { get; init; }
	
	public SyntaxKind SyntaxKind => SyntaxKind.NamespaceClauseNode;

	#if DEBUG
	~NamespaceClauseNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespaceClauseNode--;
	}
	#endif
}
