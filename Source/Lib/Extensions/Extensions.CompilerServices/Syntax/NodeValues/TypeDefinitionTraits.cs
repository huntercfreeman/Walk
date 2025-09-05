using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Values;

public struct TypeDefinitionTraits
{
    public TypeDefinitionTraits(
        int indexPartialTypeDefinition,
        TypeReference inheritedTypeReference,
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
    public TypeReference InheritedTypeReference { get; set; }
    public AccessModifierKind AccessModifierKind { get; set; }
    public StorageModifierKind StorageModifierKind { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; set; }
}
