using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// <see cref="TypeClauseNode"/> is used anywhere a type is referenced.
/// </summary>
public sealed class TypeClauseNode : IGenericParameterNode
{
	public TypeClauseNode(
		SyntaxToken typeIdentifier,
		GenericParameterListing genericParameterListing,
		bool isKeywordType)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeClauseNode++;
		#endif
	
		IsKeywordType = isKeywordType;
		TypeIdentifierToken = typeIdentifier;
		GenericParameterListing = genericParameterListing;
	}
	
	/// <summary>
	/// Various UI events can result in a 'TypeReference' needing to be shown on the UI.
	///
	/// In order to do this however, you'd have to cast 'TypeReference' as 'ISyntax',
	/// and this would cause boxing.
	///
	/// It is presumably preferred to just eat an "object-y" cost just once by creating a 'TypeClauseNode'.
	/// Lest it get boxed, unboxed, and boxed again -- over and over.
	///
	/// I'm going to use this constructor in the CSharpBinder expression logic temporarily so I can get it build.
	/// But any usage of this kind should probably be removed.
	/// </summary>
	public TypeClauseNode(TypeReference typeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeClauseNode++;
		#endif
	
		IsKeywordType = typeReference.IsKeywordType;
		TypeIdentifierToken = typeReference.TypeIdentifierToken;
		GenericParameterListing = typeReference.GenericParameterListing;
		ExplicitDefinitionTextSpan = typeReference.ExplicitDefinitionTextSpan;
		ExplicitDefinitionResourceUri = typeReference.ExplicitDefinitionResourceUri;
	}

	private bool _isFabricated;

	/// <summary>
	/// Given: 'int x = 2;'<br/>
	/// Then: 'int' is the <see cref="TypeIdentifierToken"/>
	/// And: <see cref="GenericParametersListingNode"/> would be null
	/// </summary>
	public SyntaxToken TypeIdentifierToken { get; set; }
	/// <summary>
	/// Given: 'int[] x = 2;'<br/>
	/// Then: 'Array&lt;T&gt;' is the <see cref="TypeIdentifierToken"/><br/>
	/// And: '&lt;int&gt;' is the <see cref="GenericParametersListingNode"/>
	/// </summary>
	public GenericParameterListing GenericParameterListing { get; set; }

	public bool IsKeywordType { get; set; }

	public TypeKind TypeKind { get; set; }

	TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

	public bool HasQuestionMark { get; set; }
	public int ArrayRank { get; set; }
	
	public bool IsBeingUsed { get; set; } = false;
	
	public TextEditorTextSpan ExplicitDefinitionTextSpan { get; set; }
	public ResourceUri ExplicitDefinitionResourceUri { get; set; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated
	{
		get
		{
			return _isFabricated;
		}
		init
		{
			_isFabricated = value;
		}
	}
	
	public SyntaxKind SyntaxKind => SyntaxKind.TypeClauseNode;
	
	public bool IsParsingGenericParameters { get; set; }

	public void SetSharedInstance(
		SyntaxToken typeIdentifier,
		GenericParameterListing genericParameterListing,
		bool isKeywordType)
	{
		IsBeingUsed = true;
	
		TypeIdentifierToken = typeIdentifier;
		GenericParameterListing = genericParameterListing;
		IsKeywordType = isKeywordType;
		TypeKind = TypeKind.None;
		HasQuestionMark = false;
		ArrayRank = 0;
		_isFabricated = false;
		IsParsingGenericParameters = false;
		ExplicitDefinitionTextSpan = default;
		ExplicitDefinitionResourceUri = default;
	}

	#if DEBUG	
	~TypeClauseNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeClauseNode--;
	}
	#endif
}