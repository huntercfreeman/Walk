using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct NamespaceGroup
{
	public NamespaceGroup(
		string namespaceString,
		List<NamespaceStatementNode> namespaceStatementNodeList)
	{
		NamespaceString = namespaceString;
		_namespaceStatementNodeList = namespaceStatementNodeList;
	}
	
	private bool _isDirtyTypeDefinitionNodeList = true;
	private List<TypeDefinitionNode> _typeDefinitionNodeList;
	
	private List<NamespaceStatementNode> _namespaceStatementNodeList = new();

	public string NamespaceString { get; }
	public IReadOnlyList<NamespaceStatementNode> NamespaceStatementNodeList => _namespaceStatementNodeList;
	
	public IReadOnlyList<TypeDefinitionNode> TypeDefinitionNodeList
	{
		get
		{
			if (_isDirtyTypeDefinitionNodeList)
			{
				_typeDefinitionNodeList = NamespaceStatementNodeList
					.SelectMany(x => x.GetTopLevelTypeDefinitionNodes())
					.ToList();
				_isDirtyTypeDefinitionNodeList = false;
			}
			
			return _typeDefinitionNodeList;
		}
	}

	public bool ConstructorWasInvoked => NamespaceStatementNodeList is not null;

	/// <summary>
	/// TODO: Ensure the namespace statements are only added after the codeblock is parsed.
	/// Otherwise the `_isDirtyTypeDefinitionNodeList` won't properly reflect the state.
	/// </summary>
	public void AddNamespaceStatementNode(NamespaceStatementNode namespaceStatementNode)
	{
		_namespaceStatementNodeList.Add(namespaceStatementNode);
		_isDirtyTypeDefinitionNodeList = true;
	}
	
	public void RemoveAtNamespaceStatementNode(int index)
	{
		_namespaceStatementNodeList.RemoveAt(index);
		_isDirtyTypeDefinitionNodeList = true;
	}
}
