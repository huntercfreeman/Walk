using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.RenderStates.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.Json.SyntaxActors;

public class TextEditorJsonLexer
{
	public static LexerKeywords LexerKeyWords = LexerKeywords.Empty;
	
	private readonly TextEditorService _textEditorService;

    public TextEditorJsonLexer(
    	TextEditorService textEditorService,
    	ResourceUri resourceUri,
    	string sourceText)
    {
    	_textEditorService = textEditorService;
    	
    	ResourceUri = resourceUri;
    	SourceText = sourceText;
    }

    public Key<RenderState> ModelRenderStateKey { get; private set; } = Key<RenderState>.Empty;

	public ResourceUri ResourceUri { get; }
	public string SourceText { get; }
	public List<SyntaxToken> SyntaxTokenList { get; } = new();

    public void Lex()
    {
        var jsonSyntaxUnit = JsonSyntaxTree.ParseText(
        	_textEditorService.__StringWalker,
            ResourceUri,
            SourceText);
        
        var syntaxNodeRoot = jsonSyntaxUnit.JsonDocumentSyntax;

        var syntaxWalker = new JsonSyntaxWalker();
        syntaxWalker.Visit(syntaxNodeRoot);

        SyntaxTokenList.AddRange(
            syntaxWalker.PropertyKeySyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.BooleanSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.IntegerSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.NullSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.NumberSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        SyntaxTokenList.AddRange(
            syntaxWalker.StringSyntaxes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));
    }
}