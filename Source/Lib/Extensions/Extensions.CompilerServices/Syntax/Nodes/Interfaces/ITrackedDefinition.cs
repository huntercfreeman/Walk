namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface ITrackedDefinition : ISyntaxNode
{
	public string IdentifierText { get; }
}
