using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class ConstructorDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode
{
    public ConstructorDefinitionNode(
        TypeReferenceValue returnTypeReference,
        SyntaxToken functionIdentifier,
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
        ReturnTypeReference = returnTypeReference;
        FunctionIdentifier = functionIdentifier;
        
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

    public SyntaxToken FunctionIdentifier { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionArgumentEntryList { get; set; }
    public int LengthFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    public ResourceUri ResourceUri { get; set; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ConstructorDefinitionNode;
    
    TypeReferenceValue IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
    public TypeReferenceValue ReturnTypeReference { get; set; }
}
