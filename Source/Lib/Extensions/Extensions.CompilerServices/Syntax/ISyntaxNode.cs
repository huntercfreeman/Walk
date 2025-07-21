namespace Walk.Extensions.CompilerServices.Syntax;

public interface ISyntaxNode : ISyntax
{
    public int Unsafe_ParentIndexKey { get; set; }
}
