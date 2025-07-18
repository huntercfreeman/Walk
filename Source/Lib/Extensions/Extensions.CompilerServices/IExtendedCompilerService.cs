using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilerService : ICompilerService
{
	public ISyntaxNode? GetSyntaxNode(int positionIndex, ResourceUri resourceUri, ICompilerServiceResource? compilerServiceResource);
	public ISyntaxNode? GetDefinitionNode(TextEditorTextSpan textSpan, ICompilerServiceResource compilerServiceResource, Symbol? symbol = null);
    public ICodeBlockOwner? GetScopeByPositionIndex(ResourceUri resourceUri, int positionIndex);
    public string GetIdentifierText(ISyntaxNode node, ResourceUri resourceUri);
}
