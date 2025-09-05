using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct TypeDefinitionTraits
{
    public TypeDefinitionTraits(
        int indexPartialTypeDefinition,
        TypeReferenceValue inheritedTypeReference,
        AccessModifierKind accessModifierKind,
        StorageModifierKind storageModifierKind,
        SyntaxToken openAngleBracketToken)
    {
        IndexPartialTypeDefinition = indexPartialTypeDefinition;
        InheritedTypeReference = inheritedTypeReference;
        AccessModifierKind = accessModifierKind;
        StorageModifierKind = storageModifierKind;
        OpenAngleBracketToken = openAngleBracketToken;
    }
    
    public int IndexPartialTypeDefinition { get; set; } = -1;
    public TypeReferenceValue InheritedTypeReference { get; set; }
    public AccessModifierKind AccessModifierKind { get; set; }
    public StorageModifierKind StorageModifierKind { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; set; }
}
