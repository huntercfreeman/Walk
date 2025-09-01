using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class FunctionInvocationNode : IInvocationNode, IGenericParameterNode
{
    public FunctionInvocationNode(
        SyntaxToken functionInvocationIdentifierToken,        
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int indexFunctionParameterEntryList,
        int countFunctionParameterEntryList,
        SyntaxToken closeParenthesisToken,
        TypeReference resultTypeReference)
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
    }

    public SyntaxToken FunctionInvocationIdentifierToken { get; set; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionParameterEntryList { get; set; }
    public int CountFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public TypeReference ResultTypeReference { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public int IdentifierStartInclusiveIndex => FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex;

    public int ParentScopeOffset { get; set; }
    
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
