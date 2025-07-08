using Walk.TextEditor.RazorLib;
namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class UnaryOperatorNode : ISyntaxNode
{
	public UnaryOperatorNode(
		TypeReference operandTypeReference,
		SyntaxToken operatorToken,
		TypeReference resultTypeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.UnaryOperatorNode++;
		#endif
	
		OperandTypeReference = operandTypeReference;
		OperatorToken = operatorToken;
		ResultTypeReference = resultTypeReference;
	}

	public TypeReference OperandTypeReference { get; }
	public SyntaxToken OperatorToken { get; }
	public TypeReference ResultTypeReference { get; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.UnaryOperatorNode;

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(UnaryOperatorNode);

#if DEBUG
	~UnaryOperatorNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.UnaryOperatorNode--;
	}
	#endif
}