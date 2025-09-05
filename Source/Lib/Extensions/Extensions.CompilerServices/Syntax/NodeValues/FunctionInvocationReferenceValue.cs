using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public record struct FunctionInvocationReferenceValue
{
    public static FunctionInvocationReferenceValue Empty { get; } = default;

    public FunctionInvocationReferenceValue(
        SyntaxToken functionInvocationIdentifierToken,
        
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        SyntaxToken openParenthesisToken,
        int indexFunctionParameterEntryList,
        int countFunctionParameterEntryList,
        SyntaxToken closeParenthesisToken,
        
        
        TypeReferenceValue resultTypeReference,
        bool isFabricated)
    {
        FunctionInvocationIdentifierToken = functionInvocationIdentifierToken;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        OpenParenthesisToken = openParenthesisToken;
        IndexFunctionParameterEntryList = indexFunctionParameterEntryList;
        CountFunctionParameterEntryList = countFunctionParameterEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        
        ResultTypeReference = resultTypeReference;
        IsFabricated = isFabricated;
    }
    
    public FunctionInvocationReferenceValue(FunctionInvocationNode functionInvocationNode)
    {
        // functionInvocationNode.IsBeingUsed = false;
    
        FunctionInvocationIdentifierToken = functionInvocationNode.FunctionInvocationIdentifierToken;
        
        OpenAngleBracketToken = functionInvocationNode.OpenAngleBracketToken;
        IndexGenericParameterEntryList = functionInvocationNode.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = functionInvocationNode.CountGenericParameterEntryList;
        CloseAngleBracketToken = functionInvocationNode.CloseAngleBracketToken;
        
        OpenParenthesisToken = functionInvocationNode.OpenParenthesisToken;
        IndexFunctionParameterEntryList = functionInvocationNode.IndexFunctionParameterEntryList;
        CountFunctionParameterEntryList = functionInvocationNode.CountFunctionParameterEntryList;
        CloseParenthesisToken = functionInvocationNode.CloseParenthesisToken;
        
        ResultTypeReference = functionInvocationNode.ResultTypeReference;
        IsFabricated = functionInvocationNode.IsFabricated;
    }

    public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionParameterEntryList { get; set; }
    public int CountFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public TypeReferenceValue ResultTypeReference { get; set; }
    public bool IsFabricated { get; set; }
}
