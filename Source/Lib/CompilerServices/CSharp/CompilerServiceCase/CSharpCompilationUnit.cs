using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.CompilerServices.CSharp.CompilerServiceCase;

/// <summary>
/// I think I want to get rid of the 'IBinderSession'.
/// Some of its properties were moved to the 'CSharpCompilationUnit'
/// and some to the 'CSharpParserModel'.
///
/// This was done based on whether the data should continue to exist
/// after the parse finished, or if the data should be cleared immediately after the parse finishes
/// (respectively).
/// </summary>
public sealed class CSharpCompilationUnit : IExtendedCompilationUnit, ICompilerServiceResource
{
    public CSharpCompilationUnit(ResourceUri resourceUri, string sourceText, CompilationUnitKind compilationUnitKind)
    {
        ResourceUri = resourceUri;
        SourceText = sourceText;
        CompilationUnitKind = compilationUnitKind;
    }
    
    public IEnumerable<TextEditorTextSpan> GetDiagnosticTextSpans()
    {
        return DiagnosticList.Select(x => x.TextSpan);
    }
    
    ICompilationUnit ICompilerServiceResource.CompilationUnit { get => this; set => _ = value; }
    
    public CompilationUnitKind CompilationUnitKind { get; }

    public ResourceUri ResourceUri { get; set; }
    public string SourceText { get; set; }
    
    public List<TextEditorDiagnostic> __DiagnosticList { get; } = new();
    public List<Symbol> __SymbolList { get; set; } = new();
    
    /// <summary>This needs to initialize to -1 to signify there are no entries in the pooled list.</summary>
    public int IndexFunctionInvocationParameterMetadataList { get; set; } = -1;
    public int CountFunctionInvocationParameterMetadataList { get; set; }
    
    public List<ICodeBlockOwner> CodeBlockOwnerList { get; } = new();
    public List<ISyntaxNode> NodeList { get; } = new();
    
    public IReadOnlyList<TextEditorDiagnostic> DiagnosticList => __DiagnosticList;
    public IReadOnlyList<Symbol> SymbolList => __SymbolList;
}
