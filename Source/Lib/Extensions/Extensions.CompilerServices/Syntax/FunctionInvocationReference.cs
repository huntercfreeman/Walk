using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct FunctionInvocationReference
{
    public static FunctionInvocationReference Empty { get; } = default;

    public FunctionInvocationReference(
        SyntaxToken functionInvocationIdentifierToken,
        
        SyntaxToken openAngleBracketToken,
        List<GenericParameterEntry> genericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        FunctionParameterListing functionParameterListing,
        TypeReference resultTypeReference,
        bool isFabricated)
    {
        FunctionInvocationIdentifierToken = functionInvocationIdentifierToken;
        
        OpenAngleBracketToken = openAngleBracketToken;
        GenericParameterEntryList = genericParameterEntryList;
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
        GenericParameterEntryList = functionInvocationNode.GenericParameterEntryList;
        CloseAngleBracketToken = functionInvocationNode.CloseAngleBracketToken;
        
        FunctionParameterListing = functionInvocationNode.FunctionParameterListing;
        ResultTypeReference = functionInvocationNode.ResultTypeReference;
        IsFabricated = functionInvocationNode.IsFabricated;
    }

    public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; }
    public List<GenericParameterEntry> GenericParameterEntryList { get; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    public FunctionParameterListing FunctionParameterListing { get; set; }
    public TypeReference ResultTypeReference { get; set; }
    public bool IsFabricated { get; set; }
}
