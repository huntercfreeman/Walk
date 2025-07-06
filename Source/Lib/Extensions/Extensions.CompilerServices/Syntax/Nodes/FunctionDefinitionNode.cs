using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// TODO: Track the open and close braces for the function body.
/// </summary>
public sealed class FunctionDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode, IGenericParameterNode, ITrackedDefinition
{
	public FunctionDefinitionNode(
		AccessModifierKind accessModifierKind,
		TypeReference returnTypeReference,
		SyntaxToken functionIdentifierToken,
		GenericParameterListing genericParameterListing,
		FunctionArgumentListing functionArgumentListing,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionDefinitionNode++;
		#endif
	
		AccessModifierKind = accessModifierKind;
		ReturnTypeReference = returnTypeReference;
		FunctionIdentifierToken = functionIdentifierToken;
		GenericParameterListing = genericParameterListing;
		FunctionArgumentListing = functionArgumentListing;
		// CodeBlock = codeBlock;
	}

	public AccessModifierKind AccessModifierKind { get; }
	public TypeReference ReturnTypeReference { get; }
	public SyntaxToken FunctionIdentifierToken { get; }
	public GenericParameterListing GenericParameterListing { get; set; }
	public FunctionArgumentListing FunctionArgumentListing { get; set; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.FunctionDefinitionNode;
	
	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
	
	public bool IsParsingGenericParameters { get; set; }
	
	public string IdentifierText => FunctionIdentifierToken.TextSpan.Text;
	public int ParentScopeIndexKey { get; set; }
	
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
