using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class VariableDeclarationNode : IExpressionNode
{
	public VariableDeclarationNode(
		TypeReference typeReference,
		SyntaxToken identifierToken,
		VariableKind variableKind,
		bool isInitialized,
		ResourceUri resourceUri)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.VariableDeclarationNode++;
		#endif
	
		TypeReference = typeReference;
		IdentifierToken = identifierToken;
		VariableKind = variableKind;
		IsInitialized = isInitialized;
		ResourceUri = resourceUri;
	}

	public TypeReference TypeReference { get; private set; }

	public SyntaxToken IdentifierToken { get; }
	/// <summary>
	/// TODO: Remove the 'set;' on this property
	/// </summary>
	public VariableKind VariableKind { get; set; }
	public bool IsInitialized { get; set; }
	public ResourceUri ResourceUri { get; set; }
	/// <summary>
	/// TODO: Remove the 'set;' on this property
	/// </summary>
	public bool HasGetter { get; set; }
	/// <summary>
	/// TODO: Remove the 'set;' on this property
	/// </summary>
	public bool GetterIsAutoImplemented { get; set; }
	/// <summary>
	/// TODO: Remove the 'set;' on this property
	/// </summary>
	public bool HasSetter { get; set; }
	/// <summary>
	/// TODO: Remove the 'set;' on this property
	/// </summary>
	public bool SetterIsAutoImplemented { get; set; }

	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.VariableDeclarationNode;
	
	public string IdentifierText(string sourceText, TextEditorService textEditorService) => IdentifierToken.TextSpan.Text(sourceText, textEditorService);

	public VariableDeclarationNode SetImplicitTypeReference(TypeReference typeReference)
	{
		typeReference.IsImplicit = true;
		TypeReference = typeReference;
		return this;
	}

	#if DEBUG	
	~VariableDeclarationNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.VariableDeclarationNode--;
	}
	#endif
}
