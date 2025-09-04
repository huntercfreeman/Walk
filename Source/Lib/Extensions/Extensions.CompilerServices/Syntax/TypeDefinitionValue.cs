using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct TypeDefinitionValue
{
    public TypeDefinitionValue(
        int indexPartialTypeDefinition,
        TypeReference inheritedTypeReference,
        AccessModifierKind accessModifierKind,
        StorageModifierKind storageModifierKind)
    {
        IndexPartialTypeDefinition = indexPartialTypeDefinition;
        InheritedTypeReference = inheritedTypeReference;
        AccessModifierKind = accessModifierKind;
        StorageModifierKind = storageModifierKind;
    }
    
    public int IndexPartialTypeDefinition { get; set; } = -1;
    public TypeReference InheritedTypeReference { get; set; }
    public AccessModifierKind AccessModifierKind { get; set; }
    public StorageModifierKind StorageModifierKind { get; set; }
}
