namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface ITrackedDefinition : ISyntaxNode
{
	public int ParentScopeIndexKey { get; }
	public string IdentifierText { get; }
}
