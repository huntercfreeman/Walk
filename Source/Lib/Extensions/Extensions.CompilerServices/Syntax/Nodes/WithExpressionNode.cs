using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class WithExpressionNode : IExpressionNode
{
	public WithExpressionNode(VariableReference variableReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.WithExpressionNode++;
		#endif
	
		VariableReference = variableReference;
		ResultTypeReference = variableReference.ResultTypeReference;
	}

	public VariableReference VariableReference { get; }
	public TypeReference ResultTypeReference { get; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.WithExpressionNode;

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(WithExpressionNode);

#if DEBUG
	~WithExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.WithExpressionNode--;
	}
	#endif
}
