namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when invoking a syntax which contains a generic type.
/// </summary>
public struct GenericParameterEntry
{
	public GenericParameterEntry(TypeReference typeReference)
	{
		TypeReference = typeReference;
	}

	public TypeReference TypeReference { get; }
	
	public bool ConstructorWasInvoked => TypeReference != default;
}