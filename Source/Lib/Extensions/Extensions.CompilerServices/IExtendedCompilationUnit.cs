using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
	public IReadOnlyList<Symbol> SymbolList { get; }
	public Dictionary<ScopeKeyAndIdentifierHash, TypeDefinitionNode> ScopeTypeDefinitionMap { get; }
	public Dictionary<ScopeKeyAndIdentifierHash, FunctionDefinitionNode> ScopeFunctionDefinitionMap { get; }
}
