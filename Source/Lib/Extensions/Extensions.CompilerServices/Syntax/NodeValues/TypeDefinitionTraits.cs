using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct TypeDefinitionTraits
{
    public TypeDefinitionTraits(TypeDefinitionNode typeDefinitionNode)
    {
        IndexPartialTypeDefinition = typeDefinitionNode.IndexPartialTypeDefinition;
        InheritedTypeReference = typeDefinitionNode.InheritedTypeReference;
        AccessModifierKind = typeDefinitionNode.AccessModifierKind;
        StorageModifierKind = typeDefinitionNode.StorageModifierKind;
        IsKeywordType = typeDefinitionNode.IsKeywordType;

        OpenAngleBracketToken = typeDefinitionNode.OpenAngleBracketToken;
        OffsetGenericParameterEntryList = typeDefinitionNode.OffsetGenericParameterEntryList;
        LengthGenericParameterEntryList = typeDefinitionNode.LengthGenericParameterEntryList;
        CloseAngleBracketToken = typeDefinitionNode.CloseAngleBracketToken;

        OpenParenthesisToken = typeDefinitionNode.OpenParenthesisToken;
        OffsetFunctionArgumentEntryList = typeDefinitionNode.OffsetFunctionArgumentEntryList;
        LengthFunctionArgumentEntryList = typeDefinitionNode.LengthFunctionArgumentEntryList;
        CloseParenthesisToken = typeDefinitionNode.CloseParenthesisToken;
    }

    /*public TypeDefinitionTraits(
        int indexPartialTypeDefinition,
        TypeReferenceValue inheritedTypeReference,
        AccessModifierKind accessModifierKind,
        StorageModifierKind storageModifierKind,
        bool isKeywordType,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList,
        SyntaxToken closeParenthesisToken)
    {
        IndexPartialTypeDefinition = indexPartialTypeDefinition;
        InheritedTypeReference = inheritedTypeReference;
        AccessModifierKind = accessModifierKind;
        StorageModifierKind = storageModifierKind;
        IsKeywordType = isKeywordType;

        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;

        OpenParenthesisToken = openParenthesisToken;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
    }*/

    public int IndexPartialTypeDefinition { get; set; } = -1;
    public TypeReferenceValue InheritedTypeReference { get; set; }
    public AccessModifierKind AccessModifierKind { get; set; }
    public StorageModifierKind StorageModifierKind { get; set; }
    public bool IsKeywordType { get; init; }

    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }

    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionArgumentEntryList { get; set; }
    public int LengthFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
}
