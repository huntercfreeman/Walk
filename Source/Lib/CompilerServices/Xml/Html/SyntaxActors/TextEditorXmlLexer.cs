using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.Xml.Html.SyntaxActors;

public class TextEditorXmlLexer
{
	public static readonly LexerKeywords LexerKeywords = LexerKeywords.Empty;
	
	private readonly TextEditorService _textEditorService;
	
    public TextEditorXmlLexer(TextEditorService textEditorService, ResourceUri resourceUri, string sourceText)
    {
    	_textEditorService = textEditorService;
    
    	ResourceUri = resourceUri;
    	SourceText = sourceText;
    }

	public ResourceUri ResourceUri { get; }
	public string SourceText { get; }

	public List<SyntaxToken> SyntaxTokenList { get; } = new();

    public void Lex()
    {
        var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
        	_textEditorService,
            ResourceUri,
            SourceText);

        var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

        var htmlSyntaxWalker = new HtmlSyntaxWalker();
        htmlSyntaxWalker.Visit(syntaxNodeRoot);

        // Tag Names
        SyntaxTokenList.AddRange(
            htmlSyntaxWalker.TagNameNodes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        // InjectedLanguageFragmentSyntaxes
        SyntaxTokenList.AddRange(
            htmlSyntaxWalker.InjectedLanguageFragmentNodes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        // Attribute Names
        SyntaxTokenList.AddRange(
            htmlSyntaxWalker.AttributeNameNodes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        // Attribute Values
        SyntaxTokenList.AddRange(
            htmlSyntaxWalker.AttributeValueNodes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));

        // Comments
        SyntaxTokenList.AddRange(
            htmlSyntaxWalker.CommentNodes.Select(x => new SyntaxToken(SyntaxKind.BadToken, x.TextEditorTextSpan)));
            
		var endOfFileTextSpan = new TextEditorTextSpan(
            SourceText.Length,
		    SourceText.Length,
		    (byte)GenericDecorationKind.None,
		    ResourceUri,
		    SourceText,
		    getTextPrecalculatedResult: string.Empty);
		
        SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EndOfFileToken, endOfFileTextSpan));
    }
}