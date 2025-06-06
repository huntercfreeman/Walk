using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct FunctionInvocationReference
{
	public static FunctionInvocationReference Empty { get; } = default;

	public FunctionInvocationReference(
		SyntaxToken functionInvocationIdentifierToken,
		GenericParameterListing genericParameterListing,
		FunctionParameterListing functionParameterListing,
		TypeReference resultTypeReference,
		bool isFabricated)
	{
		FunctionInvocationIdentifierToken = functionInvocationIdentifierToken;
		GenericParameterListing = genericParameterListing;
		FunctionParameterListing = functionParameterListing;
		ResultTypeReference = resultTypeReference;
		IsFabricated = isFabricated;
	}
	
	public FunctionInvocationReference(FunctionInvocationNode functionInvocationNode)
	{
		// functionInvocationNode.IsBeingUsed = false;
	
		FunctionInvocationIdentifierToken = functionInvocationNode.FunctionInvocationIdentifierToken;
		GenericParameterListing = functionInvocationNode.GenericParameterListing;
		FunctionParameterListing = functionInvocationNode.FunctionParameterListing;
		ResultTypeReference = functionInvocationNode.ResultTypeReference;
		IsFabricated = functionInvocationNode.IsFabricated;
	}

	public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
	public GenericParameterListing GenericParameterListing { get; set; }
	public FunctionParameterListing FunctionParameterListing { get; set; }
	public TypeReference ResultTypeReference { get; set; }
	public bool IsFabricated { get; set; }
}
