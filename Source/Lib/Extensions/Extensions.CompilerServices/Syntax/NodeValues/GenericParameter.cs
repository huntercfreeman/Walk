namespace Walk.Extensions.CompilerServices.Syntax.Values;

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
