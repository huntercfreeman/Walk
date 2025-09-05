using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct FunctionDefinitionTraits
{
    public FunctionDefinitionTraits(FunctionDefinitionNode functionDefinitionNode)
    {
        ReturnTypeReference = functionDefinitionNode.ReturnTypeReference;
        AccessModifierKind = functionDefinitionNode.AccessModifierKind;
        OpenAngleBracketToken = functionDefinitionNode.OpenAngleBracketToken;
        IndexGenericParameterEntryList = functionDefinitionNode.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = functionDefinitionNode.CountGenericParameterEntryList;
        CloseAngleBracketToken = functionDefinitionNode.CloseAngleBracketToken;
        OpenParenthesisToken = functionDefinitionNode.OpenParenthesisToken;
        IndexFunctionArgumentEntryList = functionDefinitionNode.IndexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = functionDefinitionNode.CountFunctionArgumentEntryList;
        CloseParenthesisToken = functionDefinitionNode.CloseParenthesisToken;
        IndexMethodOverloadDefinition = functionDefinitionNode.IndexMethodOverloadDefinition;
    }
    
    /*public FunctionDefinitionTraits(
        TypeReferenceValue returnTypeReference,
        AccessModifierKind accessModifierKind,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList,
        SyntaxToken closeParenthesisToken,
        int indexMethodOverloadDefinition)
    {
        ReturnTypeReference = returnTypeReference;
        AccessModifierKind = accessModifierKind;
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        OpenParenthesisToken = openParenthesisToken;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        IndexMethodOverloadDefinition = indexMethodOverloadDefinition;
    }*/

    public TypeReferenceValue ReturnTypeReference { get; set; }

    public AccessModifierKind AccessModifierKind { get; }

    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }

    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public int IndexMethodOverloadDefinition { get; set; } = -1;
}
