using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilerService : ICompilerService
{
	public ISyntaxNode? GetSyntaxNode(int positionIndex, ResourceUri resourceUri, ICompilerServiceResource? compilerServiceResource);
	public ISyntaxNode? GetDefinitionNode(TextEditorTextSpan textSpan, ICompilerServiceResource compilerServiceResource, Symbol? symbol = null);
    public TextEditorTextSpan? GetDefinitionTextSpan(TextEditorTextSpan textSpan, ICompilerServiceResource compilerServiceResource);
    public Scope GetScopeByPositionIndex(ResourceUri resourceUri, int positionIndex);
    /// <summary>
    /// This method presumes that all `TextEditorTextSpan` for string literals will store `string.Empty` as the `TextEditorTextSpan.Text`.
    /// Doing this avoids allocating a string for each of the string literals, and you can still lazily get the string on demand.
    /// </summary>
    public string GetTextFromStringLiteralToken(SyntaxToken stringLiteralToken);
}
