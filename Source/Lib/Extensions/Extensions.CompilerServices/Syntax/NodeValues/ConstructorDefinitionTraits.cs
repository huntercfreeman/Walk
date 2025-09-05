using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct ConstructorDefinitionTraits
{
    public ConstructorDefinitionTraits(ConstructorDefinitionNode constructorDefinitionNode)
    {
        ReturnTypeReference = constructorDefinitionNode.ReturnTypeReference;
        OpenAngleBracketToken = constructorDefinitionNode.OpenAngleBracketToken;
        IndexGenericParameterEntryList = constructorDefinitionNode.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = constructorDefinitionNode.CountGenericParameterEntryList;
        OpenParenthesisToken = constructorDefinitionNode.OpenParenthesisToken;
        IndexFunctionArgumentEntryList = constructorDefinitionNode.IndexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = constructorDefinitionNode.CountFunctionArgumentEntryList;
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
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }

    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
}
