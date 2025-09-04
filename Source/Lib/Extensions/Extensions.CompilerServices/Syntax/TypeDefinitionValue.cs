using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct TypeDefinitionValue
{
    public TypeDefinitionValue(
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
