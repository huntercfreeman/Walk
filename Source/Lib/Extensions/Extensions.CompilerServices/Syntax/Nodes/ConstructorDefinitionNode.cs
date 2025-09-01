using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ConstructorDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode
{
    public ConstructorDefinitionNode(
        TypeReference returnTypeReference,
        SyntaxToken functionIdentifier,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList,
        SyntaxToken closeParenthesisToken,
        ResourceUri resourceUri)
    {
        ReturnTypeReference = returnTypeReference;
        FunctionIdentifier = functionIdentifier;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        OpenParenthesisToken = openParenthesisToken;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        ResourceUri = resourceUri;
    }

    public SyntaxToken FunctionIdentifier { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    public ResourceUri ResourceUri { get; set; }

    public int ParentIndexKey { get; set; } = -1;
    public int SelfIndexKey { get; set; } = -1;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ConstructorDefinitionNode;
    
    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
    public TypeReference ReturnTypeReference { get; set; }
}
