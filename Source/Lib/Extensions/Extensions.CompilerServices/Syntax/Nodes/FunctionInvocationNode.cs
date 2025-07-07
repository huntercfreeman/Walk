using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class FunctionInvocationNode : IInvocationNode, IGenericParameterNode
{
	public FunctionInvocationNode(
		SyntaxToken functionInvocationIdentifierToken,
		GenericParameterListing genericParameterListing,
		FunctionParameterListing functionParameterListing,
		TypeReference resultTypeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionInvocationNode++;
		#endif
	
		FunctionInvocationIdentifierToken = functionInvocationIdentifierToken;
		GenericParameterListing = genericParameterListing;
		FunctionParameterListing = functionParameterListing;
		ResultTypeReference = resultTypeReference;
	}

	public SyntaxToken FunctionInvocationIdentifierToken { get; }
	public GenericParameterListing GenericParameterListing { get; set; }
	public FunctionParameterListing FunctionParameterListing { get; set; }
	public TypeReference ResultTypeReference { get; set; }

	public int ParentScopeIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.FunctionInvocationNode;
	
	public bool IsParsingFunctionParameters { get; set; }
	public bool IsParsingGenericParameters { get; set; }
	
	public TextEditorTextSpan ExplicitDefinitionTextSpan { get; set; }

	public string IdentifierText => nameof(FunctionInvocationNode);

#if DEBUG
	~FunctionInvocationNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionInvocationNode--;
	}
	#endif
}
