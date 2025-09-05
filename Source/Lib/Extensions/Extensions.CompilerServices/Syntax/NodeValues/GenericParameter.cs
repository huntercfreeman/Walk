namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

/// <summary>
/// Used when invoking a syntax which contains a generic type.
/// </summary>
public struct GenericParameter
{
    public GenericParameter(TypeReferenceValue typeReference)
    {
        TypeReference = typeReference;
    }

    public TypeReferenceValue TypeReference { get; }
    
    public bool ConstructorWasInvoked => !TypeReference.IsDefault();
}
