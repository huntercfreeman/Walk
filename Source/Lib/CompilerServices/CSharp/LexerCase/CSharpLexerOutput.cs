using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.CSharp.LexerCase;

public ref struct CSharpLexerOutput
{
	public CSharpLexerOutput(string sourceText)
    {
    	SyntaxTokenList = new();
    	MiscTextSpanList = new();
    	Text = sourceText.AsSpan();
    }
    
    public List<SyntaxToken> SyntaxTokenList { get; }
    /// <summary>
    /// MiscTextSpanList contains the comments and the escape characters.
    /// </summary>
    public List<TextEditorTextSpan> MiscTextSpanList { get; }
    public ReadOnlySpan<char> Text { get; }
}
