using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
	public IReadOnlyList<Symbol> SymbolList { get; }
	public List<ICodeBlockOwner> CodeBlockOwnerList { get; }
	public Dictionary<ScopeKeyAndIdentifierText, TypeDefinitionNode> ScopeTypeDefinitionMap { get; }
	public Dictionary<ScopeKeyAndIdentifierText, FunctionDefinitionNode> ScopeFunctionDefinitionMap { get; }
}
