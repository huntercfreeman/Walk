using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct TypeReference
{
	public TypeReference(
		SyntaxToken typeIdentifier,
		Type? valueType,
		GenericParameterListing genericParameterListing,
		bool isKeywordType,
		TypeKind typeKind,
		bool hasQuestionMark,
		int arrayRank,
		bool isFabricated)
	{
		IsKeywordType = isKeywordType;
		TypeIdentifierToken = typeIdentifier;
		ValueType = valueType;
		GenericParameterListing = genericParameterListing;
		TypeKind = typeKind;
		HasQuestionMark = hasQuestionMark;
		ArrayRank = arrayRank;
		IsFabricated = isFabricated;
	}
	
	public TypeReference(TypeClauseNode typeClauseNode)
	{
		typeClauseNode.IsBeingUsed = false;
	
		IsKeywordType = typeClauseNode.IsKeywordType;
		TypeIdentifierToken = typeClauseNode.TypeIdentifierToken;
		ValueType = typeClauseNode.ValueType;
		GenericParameterListing = typeClauseNode.GenericParameterListing;
		TypeKind = typeClauseNode.TypeKind;
		HasQuestionMark = typeClauseNode.HasQuestionMark;
		ArrayRank = typeClauseNode.ArrayRank;
		IsFabricated = typeClauseNode.IsFabricated;
	}

	public SyntaxToken TypeIdentifierToken { get; }
	public Type? ValueType { get; }
	public GenericParameterListing GenericParameterListing { get; }
	public bool IsKeywordType { get; }
	public TypeKind TypeKind { get; }
	public bool HasQuestionMark { get; }
	public int ArrayRank { get; }
	public bool IsFabricated { get; }
	
	public bool IsImplicit { get; set; }
}
