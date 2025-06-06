using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.Razor.CompilerServiceCase;
using Walk.CompilerServices.Razor.Facts;
using Walk.CompilerServices.Xml.Html.InjectedLanguage;
using Walk.CompilerServices.Xml.Html.SyntaxActors;

namespace Walk.CompilerServices.Razor;

public class RazorLexer
{
    private readonly RazorCompilerService _razorCompilerService;
    private readonly CSharpCompilerService _cSharpCompilerService;
    private readonly IEnvironmentProvider _environmentProvider;

	private static readonly LexerKeywords LexerKeywords = LexerKeywords.Empty;
	
	private readonly StringWalker _htmlStringWalker;
	
	private readonly List<SyntaxToken> _syntaxTokenList = new();
	
	private readonly TextEditorService _textEditorService;
	
	public List<SyntaxToken> SyntaxTokenList => _syntaxTokenList;

    public RazorLexer(
    	TextEditorService textEditorService,
    	StringWalker htmlStringWalker,
        ResourceUri resourceUri,
        string sourceText,
        RazorCompilerService razorCompilerService,
        CSharpCompilerService cSharpCompilerService,
        IEnvironmentProvider environmentProvider)
    {
    	_textEditorService = textEditorService;
    	_htmlStringWalker = htmlStringWalker;
        _environmentProvider = environmentProvider;
        _razorCompilerService = razorCompilerService;
        _cSharpCompilerService = cSharpCompilerService;
        
        ResourceUri = resourceUri;
        SourceText = sourceText;

        RazorSyntaxTree = new RazorSyntaxTree(_textEditorService, ResourceUri, _razorCompilerService, _cSharpCompilerService, _environmentProvider);
    }
    
    public ResourceUri ResourceUri { get; }
    public string SourceText { get; }

    public RazorSyntaxTree RazorSyntaxTree { get; private set; }

    public void Lex()
    {
        RazorSyntaxTree = new RazorSyntaxTree(_textEditorService, ResourceUri, _razorCompilerService, _cSharpCompilerService, _environmentProvider);

        InjectedLanguageDefinition razorInjectedLanguageDefinition = new(
            RazorFacts.TRANSITION_SUBSTRING,
            RazorFacts.TRANSITION_SUBSTRING_ESCAPED,
            RazorSyntaxTree.ParseInjectedLanguageFragment,
            RazorSyntaxTree.ParseTagName,
            RazorSyntaxTree.ParseAttributeName,
            RazorSyntaxTree.ParseAttributeValue);

        var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
        	_textEditorService,
        	_htmlStringWalker,
            ResourceUri,
            SourceText,
            razorInjectedLanguageDefinition);

        var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

        var htmlSyntaxWalker = new HtmlSyntaxWalker();

        htmlSyntaxWalker.Visit(syntaxNodeRoot);

        // Tag Names
        _syntaxTokenList.AddRange(
            htmlSyntaxWalker.TagNameNodes.Select(tns => new SyntaxToken(SyntaxKind.BadToken, tns.TextEditorTextSpan)));

        // InjectedLanguageFragmentSyntaxes
        _syntaxTokenList.AddRange(
            htmlSyntaxWalker.InjectedLanguageFragmentNodes.Select(ilfs => new SyntaxToken(SyntaxKind.BadToken, ilfs.TextEditorTextSpan)));

        // Attribute Names
        _syntaxTokenList.AddRange(
            htmlSyntaxWalker.AttributeNameNodes.Select(an => new SyntaxToken(SyntaxKind.BadToken, an.TextEditorTextSpan)));

        // Attribute Values
        _syntaxTokenList.AddRange(
            htmlSyntaxWalker.AttributeValueNodes.Select(av => new SyntaxToken(SyntaxKind.BadToken, av.TextEditorTextSpan)));

        // Comments
        _syntaxTokenList.AddRange(
            htmlSyntaxWalker.CommentNodes.Select(c => new SyntaxToken(SyntaxKind.BadToken, c.TextEditorTextSpan)));

        RazorSyntaxTree.ParseCodebehind();
    }
}