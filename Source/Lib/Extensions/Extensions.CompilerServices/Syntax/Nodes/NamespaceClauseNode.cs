using Walk.TextEditor.RazorLib;
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
	
	public NamespaceClauseNode(SyntaxToken identifierToken, NamespacePrefixNode namespacePrefixNode, int startOfMemberAccessChainPositionIndex)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespaceClauseNode++;
		#endif
	
		IdentifierToken = identifierToken;
		NamespacePrefixNode = namespacePrefixNode;
		StartOfMemberAccessChainPositionIndex = startOfMemberAccessChainPositionIndex;
	}

	public SyntaxToken IdentifierToken { get; set; }
	public Type? ValueType { get; set; }
	public NamespacePrefixNode? NamespacePrefixNode { get; set; }
	public int StartOfMemberAccessChainPositionIndex { get; set; }

	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	
	public SyntaxKind SyntaxKind => SyntaxKind.NamespaceClauseNode;

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(NamespaceClauseNode);

#if DEBUG
	~NamespaceClauseNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespaceClauseNode--;
	}
	#endif
}
