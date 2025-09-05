using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class FunctionInvocationNode : IInvocationNode, IGenericParameterNode
{
    public FunctionInvocationNode(
        SyntaxToken functionInvocationIdentifierToken,        
        SyntaxToken openAngleBracketToken,
        int offsetGenericParameterEntryList,
        int lengthGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int offsetFunctionParameterEntryList,
        int lengthFunctionParameterEntryList,
        SyntaxToken closeParenthesisToken,
        TypeReferenceValue resultTypeReference)
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
    }

    public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionParameterEntryList { get; set; }
    public int LengthFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public TypeReferenceValue ResultTypeReference { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public int IdentifierStartInclusiveIndex => FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex;

    public int ParentScopeSubIndex { get; set; }
    
    public bool _isFabricated;
    public bool IsFabricated
    {
        get => _isFabricated;
        init => _isFabricated = value;
    }
    public SyntaxKind SyntaxKind => SyntaxKind.FunctionInvocationNode;
    
    public bool IsParsingFunctionParameters { get; set; }
    public bool IsParsingGenericParameters { get; set; }
    
    public TextEditorTextSpan ExplicitDefinitionTextSpan { get; set; }
}
