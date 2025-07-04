using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ConstructorDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode
{
	public ConstructorDefinitionNode(
		TypeReference returnTypeReference,
		SyntaxToken functionIdentifier,
		GenericParameterListing genericParameterListing,
		FunctionArgumentListing functionArgumentListing,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorDefinitionNode++;
		#endif
	
		ReturnTypeReference = returnTypeReference;
		FunctionIdentifier = functionIdentifier;
		GenericParameterListing = genericParameterListing;
		FunctionArgumentListing = functionArgumentListing;
		// CodeBlock = codeBlock;
	}

	public TypeReference ReturnTypeReference { get; }
	public SyntaxToken FunctionIdentifier { get; }
	public GenericParameterListing GenericParameterListing { get; }
	public FunctionArgumentListing FunctionArgumentListing { get; set; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ConstructorDefinitionNode;
	
	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
	
	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return ReturnTypeReference;
	}
	#endregion

	#if DEBUG	
	~ConstructorDefinitionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorDefinitionNode--;
	}
	#endif
}
