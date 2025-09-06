using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

/// <summary>
/// TODO: Track the open and close braces for the function body.
/// </summary>
public sealed class FunctionDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode, IGenericParameterNode
{
    public FunctionDefinitionNode(
        AccessModifierKind accessModifierKind,
        TypeReferenceValue returnTypeReference,
        SyntaxToken functionIdentifierToken,
        SyntaxToken openAngleBracketToken,
        int offsetGenericParameterEntryList,
        int lengthGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int offsetFunctionArgumentEntryList,
        int lengthFunctionArgumentEntryList,
        SyntaxToken closeParenthesisToken,
        ResourceUri resourceUri)
    {
        AccessModifierKind = accessModifierKind;
        ReturnTypeReference = returnTypeReference;
        FunctionIdentifierToken = functionIdentifierToken;
        
        OpenAngleBracketToken = openAngleBracketToken;
        OffsetGenericParameterEntryList = offsetGenericParameterEntryList;
        LengthGenericParameterEntryList = lengthGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        OpenParenthesisToken = openParenthesisToken;
        OffsetFunctionArgumentEntryList = offsetFunctionArgumentEntryList;
        LengthFunctionArgumentEntryList = lengthFunctionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        ResourceUri = resourceUri;
    }

    public AccessModifierKind AccessModifierKind { get; }
    public SyntaxToken FunctionIdentifierToken { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionArgumentEntryList { get; set; }
    public int LengthFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public int IndexMethodOverloadDefinition { get; set; } = -1;

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.FunctionDefinitionNode;
    
    public bool IsParsingGenericParameters { get; set; }
    
    TypeReferenceValue IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
    public TypeReferenceValue ReturnTypeReference { get; set; }
}
