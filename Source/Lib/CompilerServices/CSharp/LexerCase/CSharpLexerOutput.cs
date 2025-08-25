using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.CSharp.LexerCase;

public ref struct CSharpLexerOutput
{
    public CSharpLexerOutput(ResourceUri resourceUri, List<SyntaxToken> syntaxTokenList, List<TextEditorTextSpan> miscTextSpanList)
    {
        ResourceUri = resourceUri;
        SyntaxTokenList = syntaxTokenList;
        MiscTextSpanList = miscTextSpanList;
    }

    public ResourceUri ResourceUri { get; }
    public List<SyntaxToken> SyntaxTokenList { get; }
    /// <summary>
    /// MiscTextSpanList contains the comments and the escape characters.
    /// </summary>
    public List<TextEditorTextSpan> MiscTextSpanList { get; }
}
