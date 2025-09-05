using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public record struct FunctionInvocationReferenceValue
{
    public static FunctionInvocationReferenceValue Empty { get; } = default;

    public FunctionInvocationReferenceValue(
        SyntaxToken functionInvocationIdentifierToken,
        SyntaxToken openAngleBracketToken,
        int offsetGenericParameterEntryList,
        int lengthGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int offsetFunctionParameterEntryList,
        int lengthFunctionParameterEntryList,
        SyntaxToken closeParenthesisToken,
        TypeReferenceValue resultTypeReference,
        bool isFabricated)
    {
        FunctionInvocationIdentifierToken = functionInvocationIdentifierToken;
        
        OpenAngleBracketToken = openAngleBracketToken;
        OffsetGenericParameterEntryList = offsetGenericParameterEntryList;
        LengthGenericParameterEntryList = lengthGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        OpenParenthesisToken = openParenthesisToken;
        OffsetFunctionParameterEntryList = offsetFunctionParameterEntryList;
        LengthFunctionParameterEntryList = lengthFunctionParameterEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        
        ResultTypeReference = resultTypeReference;
        IsFabricated = isFabricated;
    }
    
    public FunctionInvocationReferenceValue(FunctionInvocationNode functionInvocationNode)
    {
        // functionInvocationNode.IsBeingUsed = false;
    
        FunctionInvocationIdentifierToken = functionInvocationNode.FunctionInvocationIdentifierToken;
        
        OpenAngleBracketToken = functionInvocationNode.OpenAngleBracketToken;
        OffsetGenericParameterEntryList = functionInvocationNode.OffsetGenericParameterEntryList;
        LengthGenericParameterEntryList = functionInvocationNode.LengthGenericParameterEntryList;
        CloseAngleBracketToken = functionInvocationNode.CloseAngleBracketToken;
        
        OpenParenthesisToken = functionInvocationNode.OpenParenthesisToken;
        OffsetFunctionParameterEntryList = functionInvocationNode.OffsetFunctionParameterEntryList;
        LengthFunctionParameterEntryList = functionInvocationNode.LengthFunctionParameterEntryList;
        CloseParenthesisToken = functionInvocationNode.CloseParenthesisToken;
        
        ResultTypeReference = functionInvocationNode.ResultTypeReference;
        IsFabricated = functionInvocationNode.IsFabricated;
    }

    public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionParameterEntryList { get; set; }
    public int LengthFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public TypeReferenceValue ResultTypeReference { get; set; }
    public bool IsFabricated { get; set; }
}
