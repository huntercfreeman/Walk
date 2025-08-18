using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.CompilerServices.Xml.Html.Decoration;

namespace Walk.CompilerServices.Razor.CompilerServiceCase;

public class RazorResource : ICompilerServiceResource, ICompilationUnit
{
    private readonly TextEditorService _textEditorService;

    public RazorResource(
        ResourceUri resourceUri,
        RazorCompilerService razorCompilerService,
        TextEditorService textEditorService)
    {
        ResourceUri = resourceUri;
        CompilerService = razorCompilerService;
        _textEditorService = textEditorService;
    }
    
    public ResourceUri ResourceUri { get; }
    public ICompilerService CompilerService { get; }
    public ICompilationUnit CompilationUnit { get; set; }

    public List<Symbol> HtmlSymbols { get; } = new();
    
    public IReadOnlyList<SyntaxToken> TokenList { get; init; } = Array.Empty<SyntaxToken>();
    public IReadOnlyList<TextEditorDiagnostic> DiagnosticList { get; init; } = Array.Empty<TextEditorDiagnostic>();

    public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans()
    {
        return DiagnosticList.Select(x => x.TextSpan);
    }
}
