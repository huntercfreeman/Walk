namespace Walk.Extensions.CompilerServices.Syntax;

public struct TypeDefinitionValue
{
    public TypeDefinitionValue(
        int indexPartialTypeDefinition,
        TypeReference inheritedTypeReference)
    {
        IndexPartialTypeDefinition = indexPartialTypeDefinition;
        InheritedTypeReference = inheritedTypeReference;
    }
    
    public int IndexPartialTypeDefinition { get; set; } = -1;
    public TypeReference InheritedTypeReference { get; set; }
}
