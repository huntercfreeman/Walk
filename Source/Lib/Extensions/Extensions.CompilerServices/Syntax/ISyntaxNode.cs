namespace Walk.Extensions.CompilerServices.Syntax;

public interface ISyntaxNode : ISyntax
{
	public string IdentifierText { get; }
	public int ParentScopeIndexKey { get; set; }
}
