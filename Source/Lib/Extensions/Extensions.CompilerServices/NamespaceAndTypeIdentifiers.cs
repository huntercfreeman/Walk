namespace Walk.Extensions.CompilerServices;

public record struct NamespaceAndTypeIdentifiers(int NamespaceHash, int TypeIdentifierHash)
{
	private const char MEMBER_ACCESS_TEXT = '.';

	public int FullTypeNameHash => NamespaceHash + MEMBER_ACCESS_TEXT + TypeIdentifierHash;
}
