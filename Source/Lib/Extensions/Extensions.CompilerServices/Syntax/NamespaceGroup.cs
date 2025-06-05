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
			.SelectMany(x => x.TypeDefinitionNodeList);
	}
}

/*
// TODO: Consideration to caching 'GetTopLevelTypeDefinitionNodes()' was given...
// ...ultimately, the caching was done at the NamespaceStatementNode level.
// 
// There was consideration to also put the caching only on the group level.
// There are so many trade offs one way or another.
// That I'm just gonna leave this comment here, it is the code for this file that caches.
// I need to finalize a decision.
//
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
					.SelectMany(x => x.TypeDefinitionNodeList)
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
*/
