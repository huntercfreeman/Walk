using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// <see cref="TypeDefinitionNode"/> is used anywhere a type is defined.
/// </summary>
public sealed class TypeDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode, IGenericParameterNode
{
	public TypeDefinitionNode(
		AccessModifierKind accessModifierKind,
		bool hasPartialModifier,
		StorageModifierKind storageModifierKind,
		SyntaxToken typeIdentifier,
		Type? valueType,
		GenericParameterListing genericParameterListing,
		FunctionArgumentListing primaryConstructorFunctionArgumentListing,
		TypeReference inheritedTypeReference,
		string namespaceName,
		ResourceUri resourceUri
		// FindAllReferences
		// , HashSet<ResourceUri>? referenceHashSet
		)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeDefinitionNode++;
		#endif
	
		AccessModifierKind = accessModifierKind;
		HasPartialModifier = hasPartialModifier;
		StorageModifierKind = storageModifierKind;
		TypeIdentifierToken = typeIdentifier;
		ValueType = valueType;
		GenericParameterListing = genericParameterListing;
		FunctionArgumentListing = primaryConstructorFunctionArgumentListing;
		InheritedTypeReference = inheritedTypeReference;
		NamespaceName = namespaceName;
		ResourceUri = resourceUri;
		
		// FindAllReferences
		// ReferenceHashSet = referenceHashSet;
	}

	private TypeClauseNode? _toTypeClauseResult;
	
	private bool _hasCalculatedToTypeReference = false;
	private TypeReference _toTypeReferenceResult;

	public AccessModifierKind AccessModifierKind { get; }
	public bool HasPartialModifier { get; }
	public StorageModifierKind StorageModifierKind { get; }
	/// <summary>
	/// Given: 'public class Person { /* class definition here */ }'<br/>
	/// Then: 'Person' is the <see cref="TypeIdentifierToken"/><br/>
	/// And: <see cref="GenericArgumentsListingNode"/> would be null
	/// </summary>
	public SyntaxToken TypeIdentifierToken { get; }
	public Type? ValueType { get; }
	/// <summary>
	/// Given: 'public struct Array&lt;T&gt; { /* struct definition here */ }'<br/>
	/// Then: 'Array&lt;T&gt;' is the <see cref="TypeIdentifierToken"/><br/>
	/// And: '&lt;T&gt;' is the <see cref="GenericArgumentsListingNode"/>
	/// </summary>
	public GenericParameterListing GenericParameterListing { get; set; }
	public FunctionArgumentListing FunctionArgumentListing { get; set; }
	public FunctionArgumentListing PrimaryConstructorFunctionArgumentListing => FunctionArgumentListing;
	/// <summary>
	/// Given:<br/>
	/// public class Person : IPerson { ... }<br/><br/>
	/// Then: 'IPerson' is the <see cref="InheritedTypeClauseNode"/>
	/// </summary>
	public TypeReference InheritedTypeReference { get; private set; }
	public string NamespaceName { get; }
	public ResourceUri ResourceUri { get; }
	public bool IsInterface => StorageModifierKind == StorageModifierKind.Interface;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.TypeDefinitionNode;
	
	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

	/// <summary>
	/// TODO: Where is this used? ('NamespaceName' already exists and seems to be the one to keep).
	/// </summary>
	public string EncompassingNamespaceIdentifierString { get; set; }
	
	/// <summary>
	/// Any files that contain a reference to this TypeDefinitionNode should
	/// have their ResourceUri in this.
	/// </summary>
	// FindAllReferences
	// public HashSet<ResourceUri>? ReferenceHashSet { get; set; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Both;
	public int Scope_StartInclusiveIndex { get; set; } = -1;
	public int Scope_EndExclusiveIndex { get; set; } = -1;
	public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
	public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
	public int Unsafe_ParentIndexKey { get; set; } = -1;
	public int Unsafe_SelfIndexKey { get; set; } = -1;
	public bool PermitCodeBlockParsing { get; set; } = true;
	public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

	public bool IsKeywordType { get; init; }
	
	public string IdentifierText => TypeIdentifierToken.TextSpan.Text;
	
	/// <summary>
	/// TODO: TypeDefinitionNode(s) should use the expression loop to parse the...
	/// ...generic parameters. They currently use 'ParseTypes.HandleGenericArguments(...);'
	/// </summary>
	public bool IsParsingGenericParameters { get; set; }

	public void SetFunctionArgumentListing(FunctionArgumentListing functionArgumentListing)
	{
		FunctionArgumentListing = functionArgumentListing;
	}

	public TypeClauseNode ToTypeClause()
	{
		return _toTypeClauseResult ??= new TypeClauseNode(
			TypeIdentifierToken,
			ValueType,
			genericParameterListing: default,
			isKeywordType: IsKeywordType);
	}
	
	public TypeReference ToTypeReference()
	{
		if (!_hasCalculatedToTypeReference)
			_toTypeReferenceResult = new TypeReference(ToTypeClause());
		
		return _toTypeReferenceResult;
	}

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	public ICodeBlockOwner SetInheritedTypeReference(TypeReference typeReference)
	{
		InheritedTypeReference = typeReference;
		return this;
	}

	#if DEBUG	
	~TypeDefinitionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeDefinitionNode--;
	}
	#endif
}