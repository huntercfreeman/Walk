using Walk.TextEditor.RazorLib;

namespace Walk.Extensions.CompilerServices.Syntax;

public interface ISyntaxNode : ISyntax
{
	public string IdentifierText(string sourceText, TextEditorService textEditorService);
	public int Unsafe_ParentIndexKey { get; set; }
}
