using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices;

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
public struct CSharpCompilationUnit : IExtendedCompilationUnit, ICompilerServiceResource
{
    public CSharpCompilationUnit(CompilationUnitKind compilationUnitKind)
    {
        CompilationUnitKind = compilationUnitKind;
    }
    
    ICompilationUnit ICompilerServiceResource.CompilationUnit { get => this; set => _ = value; }
    
    public CompilationUnitKind CompilationUnitKind { get; }

    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int DiagnosticOffset { get; set; }
    public int DiagnosticLength { get; set; }
    
    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int SymbolOffset { get; set; }
    public int SymbolLength { get; set; }
    
    /// <summary>This needs to initialize to -1 to signify there are no entries in the pooled list.</summary>
    public int FunctionInvocationParameterMetadataOffset { get; set; } = -1;
    public int FunctionInvocationParameterMetadataLength { get; set; }
    
    /// <summary>
    /// The starting index within CSharpBinder.ScopeList that this compilation unit's scope entries reside.
    /// </summary>
    public int ScopeOffset { get; set; }
    /// <summary>
    /// The count of contiguous entries within CSharpBinder.ScopeList that correspond to this compilation unit.
    /// </summary>
    public int ScopeLength { get; set; }
    
    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int NodeOffset { get; set; }
    public int NodeLength { get; set; }

    public int NamespaceContributionOffset { get; set; }
    public int NamespaceContributionLength { get; set; }
    
    public bool IsDefault() => CompilationUnitKind == CompilationUnitKind.None;
}
