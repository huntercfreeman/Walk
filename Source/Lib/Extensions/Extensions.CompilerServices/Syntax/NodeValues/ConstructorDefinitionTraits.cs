using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct ConstructorDefinitionTraits
{
    public ConstructorDefinitionTraits(ConstructorDefinitionNode constructorDefinitionNode)
    {
        ReturnTypeReference = constructorDefinitionNode.ReturnTypeReference;
        OpenAngleBracketToken = constructorDefinitionNode.OpenAngleBracketToken;
        OffsetGenericParameterEntryList = constructorDefinitionNode.OffsetGenericParameterEntryList;
        LengthGenericParameterEntryList = constructorDefinitionNode.LengthGenericParameterEntryList;
        OpenParenthesisToken = constructorDefinitionNode.OpenParenthesisToken;
        OffsetFunctionArgumentEntryList = constructorDefinitionNode.OffsetFunctionArgumentEntryList;
        LengthFunctionArgumentEntryList = constructorDefinitionNode.LengthFunctionArgumentEntryList;
        CloseParenthesisToken = constructorDefinitionNode.CloseParenthesisToken;
    }
    
    /*public ConstructorDefinitionTraits(
        TypeReferenceValue returnTypeReference,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken openParenthesisToken,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList,
        SyntaxToken closeParenthesisToken)
    {
        ReturnTypeReference = returnTypeReference;
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        OpenParenthesisToken = openParenthesisToken;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
    }*/

    public TypeReferenceValue ReturnTypeReference { get; set; }

    public SyntaxToken OpenAngleBracketToken { get; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }

    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionArgumentEntryList { get; set; }
    public int LengthFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
}
