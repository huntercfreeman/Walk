using Walk.TextEditor.RazorLib;
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
		CodeBlock codeBlock,
		ResourceUri resourceUri)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorDefinitionNode++;
		#endif
	
		ReturnTypeReference = returnTypeReference;
		FunctionIdentifier = functionIdentifier;
		GenericParameterListing = genericParameterListing;
		FunctionArgumentListing = functionArgumentListing;
		ResourceUri = resourceUri;
	}

	public TypeReference ReturnTypeReference { get; }
	public SyntaxToken FunctionIdentifier { get; }
	public GenericParameterListing GenericParameterListing { get; }
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
	public SyntaxKind SyntaxKind => SyntaxKind.ConstructorDefinitionNode;
	
	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(ConstructorDefinitionNode);

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
