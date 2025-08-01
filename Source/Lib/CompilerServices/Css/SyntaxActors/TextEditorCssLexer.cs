using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.Css.SyntaxActors;

public class TextEditorCssLexer
{
    private static readonly LexerKeywords LexerKeywords = LexerKeywords.Empty;

    private readonly TextEditorService _textEditorService;

    public TextEditorCssLexer(TextEditorService textEditorService, ResourceUri resourceUri, string sourceText)
    {
        _textEditorService = textEditorService;
        ResourceUri = resourceUri;
        SourceText = sourceText;
    }
    
    public List<SyntaxToken> SyntaxTokenList { get; } = new();
    
    public ResourceUri ResourceUri { get; set; }
    public string SourceText { get; set; }

    public void Lex()
    {
        var cssSyntaxUnit = CssSyntaxTree.ParseText(
            _textEditorService.__StringWalker,
            ResourceUri,
            SourceText);

        var syntaxNodeRoot = cssSyntaxUnit.CssDocumentSyntax;

        var syntaxWalker = new CssSyntaxWalker();
        syntaxWalker.Visit(syntaxNodeRoot);

        SyntaxTokenList.AddRange(
            syntaxWalker.IdentifierSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.CommentSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.PropertyNameSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.PropertyValueSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));
    }
}
