using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct NamespaceGroup
{
	public NamespaceGroup(
		int namespaceHash,
		List<NamespaceStatementNode> namespaceStatementNodeList)
	{
		NamespaceHash = namespaceHash;
		NamespaceStatementNodeList = namespaceStatementNodeList;
	}

	public int NamespaceHash { get; }
	public List<NamespaceStatementNode> NamespaceStatementNodeList { get; }

	public bool ConstructorWasInvoked => NamespaceStatementNodeList is not null;

	/// <summary>
	/// <see cref="GetTopLevelTypeDefinitionNodes"/> provides a collection
	/// which contains all top level type definitions of the namespace.
	/// <br/><br/>
	/// This is to say that, any type definitions which are nested, would not
	/// be in this collection.
	/// </summary>
	public IEnumerable<TypeDefinitionNode> GetTopLevelTypeDefinitionNodes()
	{
		return NamespaceStatementNodeList
			.SelectMany(x => x.GetTopLevelTypeDefinitionNodes());
	}
}