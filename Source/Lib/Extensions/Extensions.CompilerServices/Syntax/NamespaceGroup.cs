using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct NamespaceGroup
{
	public NamespaceGroup(
		string namespaceString,
		List<NamespaceStatementNode> namespaceStatementNodeList)
	{
		NamespaceString = namespaceString;
		NamespaceStatementNodeList = namespaceStatementNodeList;
	}

	public string NamespaceString { get; }
	public List<NamespaceStatementNode> NamespaceStatementNodeList { get; }

	public bool ConstructorWasInvoked => NamespaceStatementNodeList is not null;
}
