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
        OffsetGenericParameterEntryList = functionDefinitionNode.OffsetGenericParameterEntryList;
        LengthGenericParameterEntryList = functionDefinitionNode.LengthGenericParameterEntryList;
        CloseAngleBracketToken = functionDefinitionNode.CloseAngleBracketToken;
        OpenParenthesisToken = functionDefinitionNode.OpenParenthesisToken;
        OffsetFunctionArgumentEntryList = functionDefinitionNode.OffsetFunctionArgumentEntryList;
        LengthFunctionArgumentEntryList = functionDefinitionNode.LengthFunctionArgumentEntryList;
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
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }

    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionArgumentEntryList { get; set; }
    public int LengthFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public int IndexMethodOverloadDefinition { get; set; } = -1;
}
