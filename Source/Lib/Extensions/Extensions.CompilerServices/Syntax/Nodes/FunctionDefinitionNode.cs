using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// TODO: Track the open and close braces for the function body.
/// </summary>
public sealed class FunctionDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode, IGenericParameterNode
{
	public FunctionDefinitionNode(
		AccessModifierKind accessModifierKind,
		TypeReference returnTypeReference,
		SyntaxToken functionIdentifierToken,
		GenericParameterListing genericParameterListing,
		FunctionArgumentListing functionArgumentListing,
		CodeBlock codeBlock,
		ResourceUri resourceUri)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionDefinitionNode++;
		#endif
	
		AccessModifierKind = accessModifierKind;
		ReturnTypeReference = returnTypeReference;
		FunctionIdentifierToken = functionIdentifierToken;
		GenericParameterListing = genericParameterListing;
		FunctionArgumentListing = functionArgumentListing;
		ResourceUri = resourceUri;
	}

	public AccessModifierKind AccessModifierKind { get; }
	public TypeReference ReturnTypeReference { get; }
	public SyntaxToken FunctionIdentifierToken { get; }
	public GenericParameterListing GenericParameterListing { get; set; }
	public FunctionArgumentListing FunctionArgumentListing { get; set; }
	public ResourceUri ResourceUri { get; set; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public int Scope_StartInclusiveIndex { get; set; } = -1;
	public int Scope_EndExclusiveIndex { get; set; } = -1;
	public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
	public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
	public int Unsafe_ParentIndexKey { get; set; } = -1;
	public int Unsafe_SelfIndexKey { get; set; } = -1;
	public bool PermitCodeBlockParsing { get; set; } = true;
	public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.FunctionDefinitionNode;
	
	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
	
	public bool IsParsingGenericParameters { get; set; }
	
	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return ReturnTypeReference;
	}
	#endregion

	#if DEBUG	
	~FunctionDefinitionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionDefinitionNode--;
	}
	#endif
}
