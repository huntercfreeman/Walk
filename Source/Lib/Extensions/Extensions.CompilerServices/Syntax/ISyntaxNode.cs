namespace Walk.Extensions.CompilerServices.Syntax;

public interface ISyntaxNode : ISyntax
{
	public string IdentifierText { get; }
	public int Unsafe_ParentIndexKey { get; set; }
}
