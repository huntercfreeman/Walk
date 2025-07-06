using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
	public IReadOnlyList<Symbol> SymbolList { get; }
	public Dictionary<ScopeKeyAndIdentifierTextAndSyntaxKind, ISyntaxNode> DefinitionMap { get; }
}
