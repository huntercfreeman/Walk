namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when invoking a syntax which contains a generic type.
/// </summary>
public struct GenericParameter
{
    public GenericParameter(TypeReference typeReference)
    {
        TypeReference = typeReference;
    }

    public TypeReference TypeReference { get; }
    
    public bool ConstructorWasInvoked => !TypeReference.IsDefault();
}
