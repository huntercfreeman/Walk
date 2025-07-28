using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct FunctionInvocationReference
{
    public static FunctionInvocationReference Empty { get; } = default;

    public FunctionInvocationReference(
        SyntaxToken functionInvocationIdentifierToken,
        
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        FunctionParameterListing functionParameterListing,
        TypeReference resultTypeReference,
        bool isFabricated)
    {
        FunctionInvocationIdentifierToken = functionInvocationIdentifierToken;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        FunctionParameterListing = functionParameterListing;
        ResultTypeReference = resultTypeReference;
        IsFabricated = isFabricated;
    }
    
    public FunctionInvocationReference(FunctionInvocationNode functionInvocationNode)
    {
        // functionInvocationNode.IsBeingUsed = false;
    
        FunctionInvocationIdentifierToken = functionInvocationNode.FunctionInvocationIdentifierToken;
        
        OpenAngleBracketToken = functionInvocationNode.OpenAngleBracketToken;
        IndexGenericParameterEntryList = functionInvocationNode.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = functionInvocationNode.CountGenericParameterEntryList;
        CloseAngleBracketToken = functionInvocationNode.CloseAngleBracketToken;
        
        FunctionParameterListing = functionInvocationNode.FunctionParameterListing;
        ResultTypeReference = functionInvocationNode.ResultTypeReference;
        IsFabricated = functionInvocationNode.IsFabricated;
    }

    public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    public FunctionParameterListing FunctionParameterListing { get; set; }
    public TypeReference ResultTypeReference { get; set; }
    public bool IsFabricated { get; set; }
}
